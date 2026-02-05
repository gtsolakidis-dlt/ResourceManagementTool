using System;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;

namespace WipDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            var connectionString = "Server=.\\SQLEXPRESS;Database=ResourceManagementDb;Integrated Security=true;TrustServerCertificate=True;Encrypt=False";

            try 
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // 1. Roster Check
                    Console.WriteLine("=== ROSTER CHECK ===");
                    var rosterCmd = connection.CreateCommand();
                    rosterCmd.CommandText = "SELECT Id, FullNameEn, Level FROM Roster WHERE Id = 1 OR FullNameEn LIKE '%Grigoris%'";
                    using (var reader = rosterCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"ID: {reader["Id"]}, Name: {reader["FullNameEn"]}, Level: '{reader["Level"]}'");
                        }
                    }

                    // 2. Forecast / Project Mapping
                    Console.WriteLine("\n=== FORECAST / PROJECT MAPPING ===");
                    var fwCmd = connection.CreateCommand();
                    fwCmd.CommandText = "SELECT Id, ProjectId, VersionNumber FROM ForecastVersion WHERE Id = 1";
                    int projectId = 0;
                    using (var reader = fwCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            projectId = reader.GetInt32(1);
                            Console.WriteLine($"ForecastVersionId: 1 -> ProjectId: {projectId}, Version: {reader["VersionNumber"]}");
                        }
                        else
                        {
                            Console.WriteLine("ForecastVersionId 1 NOT FOUND");
                            return;
                        }
                    }

                    // 3. Project Rates
                    Console.WriteLine($"\n=== PROJECT RATES CHECK (Project {projectId}) ===");
                    var ratesDict = new Dictionary<string, decimal>();
                    var ratesCmd = connection.CreateCommand();
                    ratesCmd.CommandText = $"SELECT Level, ActualDailyRate FROM ProjectRate WHERE ProjectId = {projectId}";
                    using (var reader = ratesCmd.ExecuteReader())
                    {
                        if (!reader.HasRows) Console.WriteLine($"NO Project Rates found for Project {projectId}!");
                        while (reader.Read())
                        {
                            var level = reader["Level"].ToString();
                            var rate = reader.GetDecimal(1);
                            Console.WriteLine($"Level: '{level}', Actual: {rate}");
                            if (!ratesDict.ContainsKey(level)) ratesDict[level] = rate;
                        }
                    }

                    // 4. Resource Allocations
                    Console.WriteLine("\n=== RESOURCE ALLOCATIONS (ForecastVersionId: 1) ===");
                    var allocations = new List<(int RosterId, DateTime Month, decimal Days)>();
                    var allocCmd = connection.CreateCommand();
                    allocCmd.CommandText = "SELECT RosterId, Month, AllocatedDays FROM ResourceAllocation WHERE ForecastVersionId = 1";
                    using (var reader = allocCmd.ExecuteReader())
                    {
                        if (!reader.HasRows) Console.WriteLine("NO Allocations found for ForecastVersionId 1");
                        while (reader.Read())
                        {
                            var rId = reader.GetInt32(0);
                            var m = reader.GetDateTime(1);
                            var d = reader.GetDecimal(2);
                            allocations.Add((rId, m, d));
                            Console.WriteLine($"RosterId: {rId}, Month: {m.ToShortDateString()}, Days: {d}");
                        }
                    }

                    // 5. Existing Snapshots
                    Console.WriteLine($"\n=== SNAPSHOTS (Project {projectId}) ===");
                    var snapCmd = connection.CreateCommand();
                    snapCmd.CommandText = $"SELECT Month, Wip, Status, UpdatedAt, ForecastVersionId FROM ProjectMonthlySnapshot WHERE ProjectId = {projectId}";
                    using (var reader = snapCmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.WriteLine($"Month: {reader.GetDateTime(0).ToShortDateString()}, WIP: {reader["Wip"]}, Status: {reader["Status"]}, Updated: {reader["UpdatedAt"]}, Version: {reader["ForecastVersionId"]}");
                        }
                    }

                    // 6. Roster Lookup
                    var rosterDict = new Dictionary<int, string>();
                    var rosterCmd2 = connection.CreateCommand();
                    rosterCmd2.CommandText = "SELECT Id, Level FROM Roster";
                    using (var reader = rosterCmd2.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rosterDict[reader.GetInt32(0)] = reader["Level"]?.ToString() ?? "";
                        }
                    }

                    // 7. Simulation
                    Console.WriteLine("\n=== CALCULATION SIMULATION ===");
                    decimal totalWipInfo = 0;
                    
                    foreach (var alloc in allocations)
                    {
                        // Check specifically for relevant months (Jan 2026)
                        if (alloc.Month.Year == 2026 && alloc.Month.Month == 1)
                        {
                            Console.WriteLine($"Processing Allocation: RosterId {alloc.RosterId}, Days {alloc.Days}");
                            
                            if (rosterDict.TryGetValue(alloc.RosterId, out var level))
                            {
                                Console.WriteLine($"  - Roster Level: '{level}'");
                                if (ratesDict.TryGetValue(level, out var rate))
                                {
                                    var wip = alloc.Days * rate;
                                    Console.WriteLine($"  - Rate found: {rate}. Calc WIP: {wip}");
                                    totalWipInfo += wip;
                                }
                                else
                                {
                                    Console.WriteLine($"  - NO RATE FOUND for level '{level}'");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"  - ROSTER NOT FOUND for Id {alloc.RosterId}");
                            }
                        }
                    }
                    Console.WriteLine($"TOTAL SIMULATED WIP FOR JAN 2026: {totalWipInfo}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }
    }
}
