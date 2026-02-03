using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace TemplateGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            VerifyFile("RosterTemplate.xlsx", "RosterExport.xlsx");
            VerifyFile("ProjectTemplate.xlsx", "ProjectExport.xlsx");
        }

        private static void VerifyFile(string templatePath, string exportPath)
        {
            Console.WriteLine($"--- Verifying {exportPath} against {templatePath} ---");
            
            if (!File.Exists(templatePath)) { Console.WriteLine($"Error: {templatePath} not found."); return; }
            if (!File.Exists(exportPath)) { Console.WriteLine($"Error: {exportPath} not found."); return; }

            using var templatePkg = new ExcelPackage(new FileInfo(templatePath));
            using var exportPkg = new ExcelPackage(new FileInfo(exportPath));

            var templateWs = templatePkg.Workbook.Worksheets[0];
            var exportWs = exportPkg.Workbook.Worksheets[0];

            var templateHeaders = GetHeaders(templateWs);
            var exportHeaders = GetHeaders(exportWs);

            Console.WriteLine($"Template Headers: {string.Join(", ", templateHeaders)}");
            Console.WriteLine($"Export Headers: {string.Join(", ", exportHeaders)}");

            var missingInExport = templateHeaders.Except(exportHeaders, StringComparer.OrdinalIgnoreCase).ToList();
            var extraInExport = exportHeaders.Except(templateHeaders, StringComparer.OrdinalIgnoreCase).ToList();

            if (!missingInExport.Any() && !extraInExport.Any())
            {
                Console.WriteLine("Headers match perfectly!");
            }
            else
            {
                if (missingInExport.Any()) Console.WriteLine($"Missing in export: {string.Join(", ", missingInExport)}");
                if (extraInExport.Any()) Console.WriteLine($"Extra in export: {string.Join(", ", extraInExport)}");
            }

            // Check if there is data
            int rowCount = exportWs.Dimension?.Rows ?? 0;
            Console.WriteLine($"Export row count: {rowCount}");
            if (rowCount > 1)
            {
                Console.WriteLine("Data found in export.");
            }
            else
            {
                Console.WriteLine("WARNING: No data found in export (only headers).");
            }
            Console.WriteLine();
        }

        private static List<string> GetHeaders(ExcelWorksheet ws)
        {
            var headers = new List<string>();
            if (ws.Dimension == null) return headers;
            for (int col = 1; col <= ws.Dimension.Columns; col++)
            {
                var val = ws.Cells[1, col].Value?.ToString();
                if (!string.IsNullOrWhiteSpace(val)) headers.Add(val);
            }
            return headers;
        }
    }
}
