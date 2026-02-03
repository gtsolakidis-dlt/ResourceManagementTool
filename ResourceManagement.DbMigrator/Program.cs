using System;
using System.IO;
using Microsoft.Data.SqlClient;

namespace ResourceManagement.DbMigrator
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=GRPC012824\\SQLEXPRESS;Integrated Security=true;Database=master;TrustServerCertificate=True;Encrypt=False";
            string dbName = "ResourceManagementDb";
            string schemaPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "ResourceManagement.Infrastructure", "Persistence", "Scripts", "InitialSchema.sql");

            try
            {
                Console.WriteLine("Ensuring database exists...");
                using (var conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = $"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = '{dbName}') CREATE DATABASE {dbName}";
                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine("Initializing schema...");
                string sqlConnStr = connectionString.Replace("Database=master", $"Database={dbName}");
                using (var conn = new SqlConnection(sqlConnStr))
                {
                    conn.Open();
                    string sql = File.ReadAllText(schemaPath);
                    
                    // Split by GO if necessary, but Dapper script is simple. 
                    // Let's try executing as is, or splitting by blocks.
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine("Database initialized successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
