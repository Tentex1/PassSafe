namespace PassSafe.Services
{
    using Microsoft.Maui.Storage;
    using PassSafe.Models;
    using SQLite;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Implements SQLite operations using SQLCipher. 
    /// Ensures the database file itself is fully encrypted at rest.
    /// </summary>
    public class DatabaseService(IDialogService dialogService) : IDatabaseService
    {
        private SQLiteAsyncConnection db;

        /// <summary>
        /// Connects to the SQLite database and encrypts the file using the provided master key.
        /// </summary>
        public async Task<bool> InitializeDatabaseAsync(string key)
        {
            // FIX: If the database is already initialized, return true instead of false.
            if (db != null) return true;

            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }

            try
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "passwords.sqlite");

                // Open the database with SQLCipher encryption enabled
                var options = new SQLiteConnectionString(
                    dbPath,
                    openFlags: SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache,
                    storeDateTimeAsTicks: true,
                    key
                );

                db = new SQLiteAsyncConnection(options);

                await db.CreateTableAsync<Password>();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves all saved passwords from the encrypted database.
        /// </summary>
        public async Task<List<Password>> GetDatabaseAsync()
        {
            try
            {
                var masterPass = await SecureStorage.GetAsync("masterPass");
                await InitializeDatabaseAsync(masterPass);

                if (db == null) return new List<Password>();

                return await db.Table<Password>().ToListAsync();
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync(ex);
                return new List<Password>();
            }
        }

        /// <summary>
        /// Inserts a new password record into the database.
        /// </summary>
        public async Task AddPasswordAsync(Password password)
        {
            var masterPass = await SecureStorage.GetAsync("masterPass");
            await InitializeDatabaseAsync(masterPass);

            if (db != null)
            {
                await db.InsertAsync(password);
            }
        }

        /// <summary>
        /// Deletes a password record by its ID.
        /// </summary>
        public async Task DeletePasswordAsync(int id)
        {
            if (db != null)
            {
                await db.DeleteAsync<Password>(id);
            }
        }

        /// <summary>
        /// Updates an existing password record in the database.
        /// </summary>
        public async Task UpdatePasswordAsync(Password password)
        {
            if (db != null)
            {
                await db.UpdateAsync(password);
            }
        }
    }
}