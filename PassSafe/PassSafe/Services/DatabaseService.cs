using PassSafe.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace PassSafe.Services
{
    public class DatabaseService : IDatabaseService
    {
        public SQLiteAsyncConnection db;

        public async Task InitializeDatabase(string key)
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "passwords.db3");
            var options = new SQLiteConnectionString(dbPath, true, key);

            db = new SQLiteAsyncConnection(options);
            await db.CreateTableAsync<Password>();
        }
        public async Task<List<Password>> GetDatabase()
        {
            return await db.Table<Password>().ToListAsync();
        }
    }
}
