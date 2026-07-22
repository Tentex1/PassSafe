namespace PassSafe.ViewModels
{
    using CommunityToolkit.Maui.Alerts;
    using CommunityToolkit.Maui.Storage;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using PassSafe.Services;
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class SettingsViewModel : ObservableObject
    {
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private bool isImportSuccessfull = false;

        [ObservableProperty]
        private bool isExportSuccessfull = false;

        [ObservableProperty]
        private string selectedTheme;

        [ObservableProperty]
        private string autoLockTime;

        [ObservableProperty]
        private string clipboardClearTime;

        public SettingsViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;

            SelectedTheme = Preferences.Get("AppTheme", "Sistem");
            AutoLockTime = Preferences.Get("AutoLockTime", "5 Dakika");
            ClipboardClearTime = Preferences.Get("ClipboardClearTime", "30 Saniye");
        }

        partial void OnSelectedThemeChanged(string value)
        {
            Preferences.Set("AppTheme", value);
            if (value == "Açık") Application.Current.UserAppTheme = AppTheme.Light;
            else if (value == "Koyu") Application.Current.UserAppTheme = AppTheme.Dark;
            else Application.Current.UserAppTheme = AppTheme.Unspecified;
        }

        partial void OnAutoLockTimeChanged(string value) => Preferences.Set("AutoLockTime", value);

        partial void OnClipboardClearTimeChanged(string value) => Preferences.Set("ClipboardClearTime", value);

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
                    await _dialogService.ShowAlertAsync("Hata", "Lütfen geçerli bir SQLite veritabanı dosyası seçin.", "Tamam");
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

                var popup = new Views.ImportDatabaseVerifyPopup();
                var popupVM = App.Services.GetService<ImportDatabaseVerifyViewModel>();

                await _dialogService.ShowPopupAsync(popup);

                if (popupVM?.IsVerified == true)
                {
                    await _dialogService.ShowAlertAsync("Başarılı!", "Veritabanınız başarıyla içe aktarıldı.", "Tamam");

                    if (backupCreated && File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Başarısız!", "Veritabanının şifresi doğrulanamadı. Değişiklikler geri alınıyor.", "Tamam");
                    RestoreBackup(backupPath, targetPath, backupCreated);
                }
            }
            catch (Exception ex)
            {
                RestoreBackup(backupPath, targetPath, backupCreated);
                await _dialogService.ShowErrorAsync(ex);
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
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "passwords.sqlite");

                if (!File.Exists(dbPath))
                {
                    var allFiles = Directory.GetFiles(FileSystem.AppDataDirectory);
                    var possibleDb = allFiles.FirstOrDefault(f => f.EndsWith(".sqlite") || f.EndsWith(".db") || f.EndsWith(".db3"));

                    if (possibleDb != null)
                    {
                        dbPath = possibleDb;
                    }
                    else
                    {
                        await _dialogService.ShowAlertAsync("Hata", "Dışa aktarılacak veritabanı bulunamadı! Kasanız şu an boş olabilir.", "Tamam");
                        return;
                    }
                }

                byte[] fileBytes;
                using (var fileStream = new FileStream(dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await fileStream.CopyToAsync(memoryStream);
                        fileBytes = memoryStream.ToArray();
                    }
                }

                using var streamToSave = new MemoryStream(fileBytes);
                var fileSaverResult = await FileSaver.Default.SaveAsync("passwords_backup.sqlite", streamToSave, CancellationToken.None);

                if (fileSaverResult.IsSuccessful)
                {
                    await _dialogService.ShowAlertAsync("Yedekleme Başarılı!", $"Veritabanı güvenle cihazınıza dışa aktarıldı.\n\nKonum: {fileSaverResult.FilePath}", "Tamam");
                }
                else
                {
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Kritik Hata", $"Dışa aktarma başarısız oldu:\n\n{ex.Message}", "Tamam");
            }
        }
    }
}