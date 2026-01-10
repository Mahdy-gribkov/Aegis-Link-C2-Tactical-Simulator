using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Dapper;

namespace AegisLink.Persistence
{
    public class MissionRepository
    {
        private readonly string _connectionString;

        public MissionRepository(string dbPath = "mission_log.db")
        {
            _connectionString = $"Data Source={dbPath}";
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            
            const string sql = @"
                CREATE TABLE IF NOT EXISTS MissionEvents (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP,
                    EventName TEXT NOT NULL,
                    Details TEXT
                );";

            connection.Execute(sql);
        }

        public async Task LogEventAsync(string eventName, string details)
        {
            using var connection = new SqliteConnection(_connectionString);
            const string sql = "INSERT INTO MissionEvents (EventName, Details) VALUES (@EventName, @Details);";
            
            await connection.ExecuteAsync(sql, new { EventName = eventName, Details = details });
        }
    }
}
