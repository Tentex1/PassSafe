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

        public async Task InitializeDatabaseAsync(string key)
        {
            if (db != null) return;

            if (string.IsNullOrWhiteSpace(key))
            {
                System.Diagnostics.Debug.WriteLine("[DB Warning] -> Ana şifre boş olduğu için DB henüz başlatılmadı.");
                return;
            }

            try
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "passwords.sqlite");
                System.Diagnostics.Debug.WriteLine($"[DB Info] -> Veritabanı yolu: {dbPath}");

                var options = new SQLiteConnectionString(
                    dbPath,
                    openFlags: SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache,
                    storeDateTimeAsTicks: true,
                    key        
                );

                db = new SQLiteAsyncConnection(options);

                await db.CreateTableAsync<Password>();
                System.Diagnostics.Debug.WriteLine("[DB Success] -> Veritabanı DOSYASI ve TABLOSU başarıyla oluşturuldu/bağlandı.");
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync(ex);
                if (ex.InnerException != null)
                {
                    await dialogService.ShowErrorAsync(ex);
                }
                db = null;
                throw;
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