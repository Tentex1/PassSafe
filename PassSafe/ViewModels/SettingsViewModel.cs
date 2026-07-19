using CommunityToolkit.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Text;

namespace PassSafe.ViewModels
{
    public partial class SettingsViewModel(IDialogService dialogService) : ObservableObject
    {

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

                string ext = Path.GetExtension(result.FullPath).ToLower();
                if (ext != ".sqlite" && ext != ".db" && ext != ".db3") return;

                // 1. Mevcut çalışan veritabanını korumak için yedeğini alıyoruz
                if (File.Exists(targetPath))
                {
                    File.Copy(targetPath, backupPath, overwrite: true);
                    backupCreated = true;
                }

                // 2. Yeni gelen veritabanını asıl dosyanın yerine yazıyoruz
                File.Copy(result.FullPath, targetPath, overwrite: true);

                // 3. Popup'ı şifre sorması için açıyoruz
                var popupPage = new SetMasterPassPopup();
                var popupVm = (SetMasterPassViewModel)popupPage.BindingContext;
                popupVm.InitializeMode(MasterPassMode.ValidateImportedDb);

                await Mopups.Services.MopupService.Instance.PushAsync(popupPage);

                // 4. Kullanıcının şifreyi girmesini bekliyoruz
                // (SetMasterPassViewModel içindeki TestSqlCipherConnection artık direkt true dönebilir, 
                // çünkü asıl testi burada senin DatabaseService yapacak!)
                bool isPasswordCorrect = await popupVm.ResultCompletionSource.Task;

                if (isPasswordCorrect)
                {
                    // ŞİFRE DOĞRU: Yedek dosyasını silebiliriz, aktarım başarılı!
                    if (backupCreated && File.Exists(backupPath))
                        File.Delete(backupPath);

                    await dialogService.ShowAlertAsync("Başarılı", "Veritabanı aktarıldı.", "Tamam");
                }
                else
                {
                    // ŞİFRE YANLIŞ VEYA İPTAL: Yeni gelen hatalı dosyayı silip eski yedeği geri yüklüyoruz
                    if (backupCreated && File.Exists(backupPath))
                    {
                        File.Copy(backupPath, targetPath, overwrite: true);
                        File.Delete(backupPath);
                    }
                    await dialogService.ShowAlertAsync("Hata", "Şifre doğrulanamadı, değişiklikler geri alındı.", "Tamam");
                }
            }
            catch (Exception ex)
            {
                // Beklenmedik bir hata olursa da eski veritabanını korumak için geri yüklüyoruz
                if (backupCreated && File.Exists(backupPath))
                {
                    File.Copy(backupPath, targetPath, overwrite: true);
                    File.Delete(backupPath);
                }
                await dialogService.ShowErrorAsync(ex);
            }
        }

        [RelayCommand]
        private async Task ExportDatabaseAsync()
        {
            try
            {
                string currentDbPath = Path.Combine(FileSystem.AppDataDirectory, "passwords.sqlite");

                if (!File.Exists(currentDbPath)) return;    

                using var fileStream = File.OpenRead(currentDbPath);

                var fileSaverResult = await FileSaver.Default.SaveAsync("passwords_yedek.sqlite", fileStream, CancellationToken.None);

                if (fileSaverResult.IsSuccessful)
                {
                    await dialogService.ShowAlertAsync("Başarılı", $"Yedek şuraya kaydedildi: {fileSaverResult.FilePath}", "OK");
                }
                else
                {
                    await dialogService.ShowAlertAsync("İptal Edildi", "Dosya kaydedilmedi.", "OK");
                }
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync(ex);
            }
        }
    }
}
