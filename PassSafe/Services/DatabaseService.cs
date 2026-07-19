using PassSafe.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace PassSafe.Services
{
    public class DatabaseService(IDialogService dialogService) : IDatabaseService
    {
        private SQLiteAsyncConnection db;

        public async Task<bool> InitializeDatabaseAsync(string key)
        {
            if (db != null) return false;

            if (string.IsNullOrWhiteSpace(key))
            {
                //await dialogService.ShowErrorAsync(null, "Ana şifre yok");
                return false;
            }

            try
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "passwords.sqlite");

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
            catch (Exception ex)
            {
                //await dialogService.ShowErrorAsync(ex);
                //if (ex.InnerException != null)
                //{
                //    await dialogService.ShowErrorAsync(ex);
                //}
                //db = null;
                return false;
            }
        }

        public async Task<List<Password>> GetDatabaseAsync()
        {
            try
            {
                var masterPass = await SecureStorage.GetAsync("master_pass");
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

        public async Task<List<Password>> GetFavoritesAsync()
        {
            try
            {
                var masterPass = await SecureStorage.GetAsync("master_pass");
                await InitializeDatabaseAsync(masterPass);

                if (db == null) return new List<Password>();

                return await db.Table<Password>().Where(x => x.IsFavorited == true).ToListAsync();
            }
            catch(Exception ex)
            {
                await dialogService.ShowErrorAsync(ex);
                return new List<Password>();
            }
        }

        public async Task AddPasswordAsync(Password password)
        {
            var masterPass = await SecureStorage.GetAsync("master_pass");
            await InitializeDatabaseAsync(masterPass);

            if (db != null)
            {
                await db.InsertAsync(password);
            }
        }

        public async Task DeletePasswordAsync(int id)
        {
            if (db != null)
            {
                await db.DeleteAsync<Password>(id);
            }
        }

        public async Task UpdatePasswordAsync(Password password)
        {
            if (db != null)
            {
                await db.UpdateAsync(password);
            }
        }
    }
}