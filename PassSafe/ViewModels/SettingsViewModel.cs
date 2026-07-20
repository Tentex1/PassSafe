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

        async partial void OnIsImportSuccessfullChanged(bool value)
        {
            if (IsImportSuccessfull == true)
            {
                var popup = new ImportDatabaseVerifyPopup();
                var popupVM = App.Services.GetService<ImportDatabaseVerifyViewModel>();

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await dialogService.ShowPopupAsync(popup);
                });

                if (popupVM?.IsVerified == true)
                {
                    await dialogService.ShowAlertAsync("Başarılı!", "Veritabanınız başarıyla içe aktarıldı.", "Tamam");
                }

            }
            else { await dialogService.ShowAlertAsync("Başarısız!", "Veritabanının şifresi yanlış.", "Tamam"); }
        }

        [RelayCommand]
        private async Task ImportDatabaseAsync()
        {
            string targetPath = Path.Combine(FileSystem.AppDataDirectory, "passwords.sqlite");
            string backupPath = Path.Combine(FileSystem.AppDataDirectory, "passwords_backup.sqlite");
            bool backupCreated = false;

            try
            {
                var result = await FilePicker.Default.PickAsync();
                if (result == null) { IsImportSuccessfull = false; return; }

                string ext = Path.GetExtension(result.FullPath).ToLower();

                if (File.Exists(targetPath))
                {
                    File.Copy(targetPath, backupPath, overwrite: true);
                    backupCreated = true;
                }

                if (ext.EndsWith("sqlite") || ext.EndsWith("db") || ext.EndsWith("db3"))
                {
                    File.Copy(result.FullPath, targetPath, overwrite: true);
                    IsImportSuccessfull = true;
                }
                else { IsImportSuccessfull = false; }
            }
            catch (Exception ex)
            {
                if (backupCreated && File.Exists(backupPath))
                {
                    File.Copy(backupPath, targetPath, overwrite: true);
                    File.Delete(backupPath);
                }
                await dialogService.ShowErrorAsync(ex);
                IsImportSuccessfull = false;
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

                // FileShare.ReadWrite sayesinde SQLite arkada açık olsa bile dosyayı okuyabilirsin
                using var stream = new FileStream(dbPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                var fileSaverResult = await FileSaver.Default.SaveAsync("passwords_backup.sqlite", stream, CancellationToken.None);

                if (fileSaverResult.IsSuccessful)
                {
                    await Toast.Make("Veritabanı başarıyla dışa aktarıldı: " + fileSaverResult.FilePath).Show();
                }
                else
                {
                    await Toast.Make("Veritabanı dışa aktarımı iptal edildi veya başarısız oldu!").Show();
                }
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync(ex);
            }
        }

    }
}
