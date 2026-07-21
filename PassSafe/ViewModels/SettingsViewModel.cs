namespace PassSafe.ViewModels
{
    using CommunityToolkit.Maui.Alerts;
    using CommunityToolkit.Maui.Storage;
    using PassSafe.Services;
    using System;
    using System.Text;

    public partial class SettingsViewModel(IDialogService dialogService) : ObservableObject
    {
        [ObservableProperty]
        private bool isImportSuccessfull = false;


        [ObservableProperty]
        private bool isExportSuccessfull = false;

        [RelayCommand]
        private async Task ImportDatabaseAsync()
        {
            string targetPath = Path.Combine(FileSystem.AppDataDirectory, "passwords.sqlite");
            string backupPath = Path.Combine(FileSystem.AppDataDirectory, "passwords_backup.sqlite");
            bool backupCreated = false;

            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result == null) return;      

                string ext = Path.GetExtension(result.FileName).ToLower();

                if (!ext.EndsWith("sqlite") && !ext.EndsWith("db") && !ext.EndsWith("db3"))
                {
                    await dialogService.ShowAlertAsync("Hata", "Lütfen geçerli bir SQLite veritabanı dosyası seçin.", "Tamam");
                    return;
                }

                if (File.Exists(targetPath))
                {
                    File.Copy(targetPath, backupPath, overwrite: true);
                    backupCreated = true;
                }

                using (var sourceStream = await result.OpenReadAsync())
                using (var targetStream = File.Create(targetPath))
                {
                    await sourceStream.CopyToAsync(targetStream);
                }

                var popup = new ImportDatabaseVerifyPopup();
                var popupVM = App.Services.GetService<ImportDatabaseVerifyViewModel>();

                await dialogService.ShowPopupAsync(popup);

                if (popupVM?.IsVerified == true)
                {
                    await dialogService.ShowAlertAsync("Başarılı!", "Veritabanınız başarıyla içe aktarıldı.", "Tamam");
                    
                    if (backupCreated && File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }
                }
                else
                {
                    await dialogService.ShowAlertAsync("Başarısız!", "Veritabanının şifresi doğrulanamadı. Değişiklikler geri alınıyor.", "Tamam");
                    RestoreBackup(backupPath, targetPath, backupCreated);
                }
            }
            catch (Exception ex)
            {
                RestoreBackup(backupPath, targetPath, backupCreated);
                await dialogService.ShowErrorAsync(ex);
            }
        }

        private void RestoreBackup(string backupPath, string targetPath, bool backupCreated)
        {
            if (backupCreated && File.Exists(backupPath))
            {
                File.Copy(backupPath, targetPath, overwrite: true);
                File.Delete(backupPath);
            }
        }

        [RelayCommand]
        private async Task ExportDatabaseAsync()
        {
            try
            {
                var dbPath = Path.Combine(FileSystem.AppDataDirectory, "passwords.sqlite");

                if (!File.Exists(dbPath))
                {
                    await Toast.Make("Veritabanı dosyası bulunamadı!").Show();
                    return;
                }

                using var stream = new FileStream(dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                var fileSaverResult = await FileSaver.Default.SaveAsync("passwords_backup.sqlite", stream, CancellationToken.None);

                if (fileSaverResult.IsSuccessful)
                {
                    await Toast.Make("Veritabanı başarıyla dışa aktarıldı: " + fileSaverResult.FilePath).Show();
                }
                else
                {
                    await Toast.Make("Veritabanı dışa aktarımı iptal edildi.").Show();
                }
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync(ex);
            }
        }

    }
}
