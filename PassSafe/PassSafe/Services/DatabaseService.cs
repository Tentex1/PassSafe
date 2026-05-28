using PassSafe.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;

namespace PassSafe.Services
{
    public class DatabaseService : IDatabaseService
    {
        // Bağlantıyı tek bir kez tutmak için private değişken yapıyoruz
        private SQLiteAsyncConnection db;

        public async Task InitializeDatabase(string key)
        {
            // 1. EĞER BAĞLANTI ZATEN VARSA TEKRAR OLUŞTURMA (Kritik Hatanın Çözümü)
            if (db != null) return;

            // 2. Şifre boş veya null ise veritabanını başlatma, hata vermesini engelle
            if (string.IsNullOrWhiteSpace(key))
            {
                System.Diagnostics.Debug.WriteLine("[DB Warning] -> Ana şifre boş olduğu için DB henüz başlatılmadı.");
                return;
            }

            try
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "passwords.db3");

                // SQLCipher için bağlantı ayarları
                var options = new SQLiteConnectionString(dbPath, storeDateTimeAsTicks: true, key: key);

                db = new SQLiteAsyncConnection(options);

                // Tabloyu oluştur
                await db.CreateTableAsync<Password>();
                System.Diagnostics.Debug.WriteLine("[DB Success] -> Veritabanı şifreli olarak başarıyla bağlandı.");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB Error] -> Başlatma hatası: {ex.Message}");
                db = null; // Hata aldıysa nesneyi sıfırla ki sonraki denemede temiz başlasın
                throw;
            }
        }

        public async Task<List<Password>> GetDatabase()
        {
            var masterPass = await SecureStorage.GetAsync("master_pass");
            await InitializeDatabase(masterPass);

            // Eğer şifre belirlenmediyse ve db null kaldıysa boş liste dön, uygulama çökmesin
            if (db == null) return new List<Password>();

            return await db.Table<Password>().ToListAsync();
        }

        public async Task AddPassword(Password password)
        {
            var masterPass = await SecureStorage.GetAsync("master_pass");
            await InitializeDatabase(masterPass);

            if (db != null)
            {
                await db.InsertAsync(password);
            }
        }

        public async Task DeletePassword(int id)
        {
            // Silme işleminden önce de db kontrolü şart
            if (db != null)
            {
                await db.DeleteAsync<Password>(id);
            }
        }
    }
}