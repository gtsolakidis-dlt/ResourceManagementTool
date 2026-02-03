using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using ResourceManagement.Domain.Interfaces;

namespace ResourceManagement.Infrastructure.Persistence.Services
{
    public class ExcelService : IExcelService
    {
        public ExcelService()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        public async Task<byte[]> ExportToExcelAsync<T>(IEnumerable<T> data, string sheetName)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add(sheetName);

            // Filter out internal fields and read-only properties
            var properties = typeof(T).GetProperties()
                .Where(p => p.CanWrite &&
                            !p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                            !p.Name.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase) &&
                            !p.Name.Equals("UpdatedAt", StringComparison.OrdinalIgnoreCase) &&
                            !p.Name.Equals("IsDeleted", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            // Load data without creating a table initially
            var dataList = data.ToList();
            if (dataList.Count == 0)
            {
                // Still create headers if no data
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i].Name;
                }
            }
            else
            {
                // Load data with headers
                for (int i = 0; i < properties.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = properties[i].Name;
                }
                
                for (int row = 0; row < dataList.Count; row++)
                {
                    var item = dataList[row];
                    for (int col = 0; col < properties.Length; col++)
                    {
                        var value = properties[col].GetValue(item);
                        worksheet.Cells[row + 2, col + 1].Value = value;
                    }
                }
            }

            if (worksheet.Dimension != null)
            {
                var dataRange = worksheet.Dimension;
                
                // Now create the table
                var tableName = $"Table_{sheetName.Replace(" ", "")}";
                var table = worksheet.Tables.Add(dataRange, tableName);
                table.TableStyle = OfficeOpenXml.Table.TableStyles.None; // We'll apply custom styling
                
                // Style header row with Deloitte dark green
                using (var headerRange = worksheet.Cells[1, 1, 1, dataRange.Columns])
                {
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Font.Size = 11;
                    headerRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(108, 165, 29)); // Deloitte dark green #6CA51D
                    headerRange.Style.Font.Color.SetColor(System.Drawing.Color.White);
                    headerRange.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    headerRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Medium;
                    headerRange.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.FromArgb(108, 165, 29));
                }

                // Apply alternating row colors with light green shades
                for (int row = 2; row <= dataRange.Rows; row++)
                {
                    using (var rowRange = worksheet.Cells[row, 1, row, dataRange.Columns])
                    {
                        if (row % 2 == 0)
                        {
                            // Even rows: Very light green
                            rowRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            rowRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(240, 250, 230)); // Very light green
                        }
                        else
                        {
                            // Odd rows: Light green
                            rowRange.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            rowRange.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(225, 245, 215)); // Light green
                        }
                        
                        // Add subtle borders
                        rowRange.Style.Border.Bottom.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;
                        rowRange.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.FromArgb(200, 230, 180));
                    }
                }

                // Apply entity-specific formatting
                var typeName = typeof(T).Name;
                
                if (typeName == "Project")
                {
                    ApplyProjectFormatting(worksheet, properties, dataRange.Rows);
                }
                else if (typeName == "Roster")
                {
                    ApplyRosterFormatting(worksheet, properties, dataRange.Rows);
                }

                // Auto-fit columns
                worksheet.Cells[dataRange.Address].AutoFitColumns();
                
                // Set minimum column width
                for (int col = 1; col <= dataRange.Columns; col++)
                {
                    if (worksheet.Column(col).Width < 10)
                        worksheet.Column(col).Width = 10;
                }
            }
            return await package.GetAsByteArrayAsync();
        }

        private void ApplyProjectFormatting(OfficeOpenXml.ExcelWorksheet worksheet, System.Reflection.PropertyInfo[] properties, int rowCount)
        {
            for (int i = 0; i < properties.Length; i++)
            {
                int colIndex = i + 1;
                var propName = properties[i].Name;

                // Columns C/D: StartDate/EndDate - Greek date format DD/MM/YYYY
                if (propName == "StartDate" || propName == "EndDate")
                {
                    using (var range = worksheet.Cells[2, colIndex, rowCount, colIndex])
                    {
                        range.Style.Numberformat.Format = "dd/mm/yyyy";
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    }
                }
                // Column E: Budget - Euro currency
                else if (propName == "Budget")
                {
                    using (var range = worksheet.Cells[2, colIndex, rowCount, colIndex])
                    {
                        range.Style.Numberformat.Format = "€#,##0.00";
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    }
                }
                // Columns F/G: Recoverability/TargetMargin - Percentage
                else if (propName == "Recoverability" || propName == "TargetMargin")
                {
                    using (var range = worksheet.Cells[2, colIndex, rowCount, colIndex])
                    {
                        range.Style.Numberformat.Format = "0.00%";
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    }
                }
            }
        }

        private void ApplyRosterFormatting(OfficeOpenXml.ExcelWorksheet worksheet, System.Reflection.PropertyInfo[] properties, int rowCount)
        {
            // Currency columns for Roster: G/H/J/K/L/M/N
            // NewAmendedSalary, EmployerContributions, TicketRestaurant, Metlife, TopusPerMonth, GrossRevenue, DiscountedRevenue
            var currencyFields = new[] { "NewAmendedSalary", "EmployerContributions", "Cars", "TicketRestaurant", 
                                        "Metlife", "TopusPerMonth", "GrossRevenue", "DiscountedRevenue" };

            for (int i = 0; i < properties.Length; i++)
            {
                int colIndex = i + 1;
                var propName = properties[i].Name;

                if (currencyFields.Contains(propName))
                {
                    using (var range = worksheet.Cells[2, colIndex, rowCount, colIndex])
                    {
                        range.Style.Numberformat.Format = "€#,##0.00";
                        range.Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Right;
                    }
                }
            }
        }

        public async Task<List<T>> ImportFromExcelAsync<T>(Stream fileStream) where T : new()
        {
            var list = new List<T>();
            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                if (worksheet.Dimension == null) return list;

                var rowCount = worksheet.Dimension.Rows;
                var colCount = worksheet.Dimension.Columns;

                var properties = typeof(T).GetProperties();
                var headerRow = 1;

                // Map headers to property names
                var headerMap = new Dictionary<int, System.Reflection.PropertyInfo>();
                for (int col = 1; col <= colCount; col++)
                {
                    var headerName = worksheet.Cells[headerRow, col].Value?.ToString();
                    if (headerName != null)
                    {
                        var prop = properties.FirstOrDefault(p => p.Name.Equals(headerName, StringComparison.OrdinalIgnoreCase));
                        if (prop != null && prop.CanWrite && 
                            !prop.Name.Equals("Id", StringComparison.OrdinalIgnoreCase) &&
                            !prop.Name.Equals("CreatedAt", StringComparison.OrdinalIgnoreCase) &&
                            !prop.Name.Equals("UpdatedAt", StringComparison.OrdinalIgnoreCase) &&
                            !prop.Name.Equals("IsDeleted", StringComparison.OrdinalIgnoreCase)) 
                        {
                            headerMap[col] = prop;
                        }
                    }
                }

                for (int row = 2; row <= rowCount; row++)
                {
                    var item = new T();
                    bool hasData = false;

                    foreach (var entry in headerMap)
                    {
                        var col = entry.Key;
                        var prop = entry.Value;
                        var cellValue = worksheet.Cells[row, col].Value;

                        if (cellValue != null)
                        {
                            try
                            {
                                object? convertedValue;
                                var targetType = prop.PropertyType;

                                // Handle Nullable types
                                if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                {
                                    targetType = Nullable.GetUnderlyingType(targetType)!;
                                }

                                if (targetType == typeof(Guid))
                                {
                                    convertedValue = Guid.Parse(cellValue.ToString()!);
                                }
                                else if (targetType.IsEnum)
                                {
                                    convertedValue = Enum.Parse(targetType, cellValue.ToString()!, true);
                                }
                                else
                                {
                                    convertedValue = Convert.ChangeType(cellValue, targetType);
                                }

                                prop.SetValue(item, convertedValue);
                                hasData = true;
                            }
                            catch { /* Skip invalid cells for robustness */ }
                        }
                    }

                    if (hasData) list.Add(item);
                }
            }
            return await Task.FromResult(list);
        }

    }
}
