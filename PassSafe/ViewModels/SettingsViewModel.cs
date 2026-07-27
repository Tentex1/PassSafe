namespace PassSafe.ViewModels
{
    using CommunityToolkit.Maui.Alerts;
    using CommunityToolkit.Maui.Storage;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Microsoft.Maui.ApplicationModel;
    using Microsoft.Maui.Graphics;
    using PassSafe.Helpers;
    using PassSafe.Services;
    using System;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class SettingItem
    {
        public string Key { get; set; }

        public string ResourceKey { get; set; }

        public string Name => LocalizationManager.Instance[ResourceKey];
    }

    public partial class SettingsViewModel : ObservableObject
    {
        public LocalizationManager Loc => LocalizationManager.Instance;

        private readonly IDialogService _dialogService;

        private readonly IDatabaseService _databaseService;

        private readonly ICryptoService _cryptoService;

        public ObservableCollection<SettingItem> ThemeOptions { get; }

        public ObservableCollection<SettingItem> ColorOptions { get; }

        public ObservableCollection<SettingItem> LanguageOptions { get; }

        public ObservableCollection<SettingItem> AutoLockOptions { get; }

        [ObservableProperty] private SettingItem selectedTheme;

        [ObservableProperty] private SettingItem selectedAccentColor;

        [ObservableProperty] private SettingItem selectedLanguage;

        [ObservableProperty] private SettingItem selectedAutoLockTime;

        public SettingsViewModel(IDialogService dialogService, IDatabaseService databaseService, ICryptoService cryptoService)
        {
            _dialogService = dialogService;
            _databaseService = databaseService;
            _cryptoService = cryptoService;

            ThemeOptions = new ObservableCollection<SettingItem>
            {
                new SettingItem { ResourceKey = "ThemeSystem", Key = "system" },
                new SettingItem { ResourceKey = "ThemeLight", Key = "light" },
                new SettingItem { ResourceKey = "ThemeDark", Key = "dark" }
            };

            ColorOptions = new ObservableCollection<SettingItem>
            {
                new SettingItem { ResourceKey = "ColorEmerald", Key = "emerald" },
                new SettingItem { ResourceKey = "ColorOcean", Key = "ocean" },
                new SettingItem { ResourceKey = "ColorFire", Key = "fire" },
                new SettingItem { ResourceKey = "ColorPurple", Key = "purple" },
                new SettingItem { ResourceKey = "ColorSun", Key = "sun" }
            };

            LanguageOptions = new ObservableCollection<SettingItem>
            {
                new SettingItem { ResourceKey = "Türkçe", Key = "tr" },
                new SettingItem { ResourceKey = "English", Key = "en" },
                new SettingItem { ResourceKey = "Русский", Key = "ru" }
            };

            AutoLockOptions = new ObservableCollection<SettingItem>
            {
                new SettingItem { ResourceKey = "TimeInstantly", Key = "0" },
                new SettingItem { ResourceKey = "Time1Min", Key = "1" },
                new SettingItem { ResourceKey = "Time5Min", Key = "5" },
                new SettingItem { ResourceKey = "TimeNever", Key = "-1" }
            };

            string savedTheme = Preferences.Get("AppTheme", "system");
            SelectedTheme = ThemeOptions.FirstOrDefault(x => x.Key == savedTheme) ?? ThemeOptions.First();

            string savedColor = Preferences.Get("AppAccentColor", "emerald");
            SelectedAccentColor = ColorOptions.FirstOrDefault(x => x.Key == savedColor) ?? ColorOptions.First();

            string savedLang = Preferences.Get("AppLanguage", "en");
            SelectedLanguage = LanguageOptions.FirstOrDefault(x => x.Key == savedLang) ?? LanguageOptions.First();

            string savedAutoLock = Preferences.Get("AutoLockTime", "5");
            SelectedAutoLockTime = AutoLockOptions.FirstOrDefault(x => x.Key == savedAutoLock) ?? AutoLockOptions[2];
        }

        partial void OnSelectedThemeChanged(SettingItem value)
        {
            if (value == null) return;
            Preferences.Set("AppTheme", value.Key);

            if (value.Key == "light") Application.Current.UserAppTheme = AppTheme.Light;
            else if (value.Key == "dark") Application.Current.UserAppTheme = AppTheme.Dark;
            else Application.Current.UserAppTheme = AppTheme.Unspecified;
        }

        partial void OnSelectedAccentColorChanged(SettingItem value)
        {
            if (value == null) return;
            Preferences.Set("AppAccentColor", value.Key);

            Color newColor;
            switch (value.Key)
            {
                case "ocean": newColor = Color.FromArgb("#3B82F6"); break;
                case "fire": newColor = Color.FromArgb("#EF4444"); break;
                case "purple": newColor = Color.FromArgb("#8B5CF6"); break;
                case "sun": newColor = Color.FromArgb("#F59E0B"); break;
                case "emerald":
                default:
                    newColor = Color.FromArgb("#10B981"); break;
            }

            Application.Current.Resources["Primary"] = newColor;
            Application.Current.Resources["PrimaryBrush"] = new SolidColorBrush(newColor);
        }

        partial void OnSelectedLanguageChanged(SettingItem value)
        {
            if (value == null) return;
            LocalizationManager.Instance.SetLanguage(value.Key);
            Preferences.Set("AppLanguage", value.Key);

            Toast.Make(Loc["LangRestartToast"]).Show();
        }

        partial void OnSelectedAutoLockTimeChanged(SettingItem value)
        {
            if (value != null) Preferences.Set("AutoLockTime", value.Key);
        }

        [RelayCommand]
        private async Task ChangeMasterPasswordAsync()
        {
            MainWindow.IsAuthenticating = true;

            try
            {
                var currentMasterPass = await SecureStorage.GetAsync("masterPass");
                if (string.IsNullOrEmpty(currentMasterPass))
                {
                    await _dialogService.ShowAlertAsync(Loc["ErrorTitle"], Loc["NoMasterPassFound"], Loc["OkBtn"]);
                    return;
                }

                string oldPassInput = await Application.Current.MainPage.DisplayPromptAsync(Loc["VerifyTitle"], Loc["EnterCurrentMasterPass"], Loc["ContinueBtn"], Loc["CancelBtn"]);
                if (oldPassInput == null) return;

                if (oldPassInput != currentMasterPass)
                {
                    await _dialogService.ShowAlertAsync(Loc["ErrorTitle"], Loc["ErrorWrongMasterPass"], Loc["OkBtn"]);
                    return;
                }

                string newPassInput = await Application.Current.MainPage.DisplayPromptAsync(Loc["NewPassTitle"], Loc["EnterNewMasterPass"], Loc["ChangeBtn"], Loc["CancelBtn"]);
                if (newPassInput == null) return;

                if (newPassInput.Length < 4)
                {
                    await _dialogService.ShowAlertAsync(Loc["ErrorTitle"], Loc["ErrorPasswordTooShort"], Loc["OkBtn"]);
                    return;
                }

                string newPassConfirm = await Application.Current.MainPage.DisplayPromptAsync(Loc["EnterAgainTitle"], Loc["ConfirmNewMasterPass"], Loc["ConfirmBtn"], Loc["CancelBtn"]);
                if (newPassConfirm == null) return;

                if (newPassInput != newPassConfirm)
                {
                    await _dialogService.ShowAlertAsync(Loc["ErrorTitle"], Loc["ErrorPasswordsNotMatch"], Loc["OkBtn"]);
                    return;
                }

                var allPasswords = await _databaseService.GetDatabaseAsync();
                if (allPasswords != null && allPasswords.Any())
                {
                    foreach (var p in allPasswords)
                    {
                        string plainText = _cryptoService.Decrypt(p.EncryptedPassword, currentMasterPass);
                        p.EncryptedPassword = _cryptoService.Encrypt(plainText, newPassInput);
                        await _databaseService.UpdatePasswordAsync(p);
                    }
                }

                await SecureStorage.SetAsync("masterPass", newPassInput);
                await _dialogService.ShowAlertAsync(Loc["SuccessTitle"], Loc["SuccessMasterPassChanged"], Loc["OkBtn"]);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync(Loc["CriticalErrorTitle"], $"{Loc["ErrorPassUpdateFailed"]}: {ex.Message}", Loc["OkBtn"]);
            }
            finally
            {
                MainWindow.IsAuthenticating = false;
            }
        }

        [RelayCommand]
        private async Task ImportDatabaseAsync()
        {
            MainWindow.IsAuthenticating = true;

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
                    await _dialogService.ShowAlertAsync(Loc["ErrorTitle"], Loc["ErrorInvalidDbFile"], Loc["OkBtn"]);
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
                    await _dialogService.ShowAlertAsync(Loc["SuccessTitle"], Loc["SuccessDbImported"], Loc["OkBtn"]);
                    if (backupCreated && File.Exists(backupPath)) File.Delete(backupPath);
                }
                else
                {
                    await _dialogService.ShowAlertAsync(Loc["FailedTitle"], Loc["ErrorDbVerifyFailed"], Loc["OkBtn"]);
                    RestoreBackup(backupPath, targetPath, backupCreated);
                }
            }
            catch (Exception ex)
            {
                RestoreBackup(backupPath, targetPath, backupCreated);
                await _dialogService.ShowErrorAsync(ex);
            }
            finally
            {
                MainWindow.IsAuthenticating = false;
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
            MainWindow.IsAuthenticating = true;

            try
            {
                string dbPath = Path.Combine(FileSystem.AppDataDirectory, "passwords.sqlite");

                if (!File.Exists(dbPath))
                {
                    var allFiles = Directory.GetFiles(FileSystem.AppDataDirectory);
                    var possibleDb = allFiles.FirstOrDefault(f => f.EndsWith(".sqlite") || f.EndsWith(".db") || f.EndsWith(".db3"));

                    if (possibleDb != null) dbPath = possibleDb;
                    else
                    {
                        await _dialogService.ShowAlertAsync(Loc["ErrorTitle"], Loc["ErrorDbNotFoundToExport"], Loc["OkBtn"]);
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
                    await _dialogService.ShowAlertAsync(Loc["SuccessBackupTitle"], $"{Loc["SuccessDbExported"]}\n\n{Loc["Location"]}: {fileSaverResult.FilePath}", Loc["OkBtn"]);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync(Loc["CriticalErrorTitle"], $"{Loc["ErrorExportFailed"]}:\n\n{ex.Message}", Loc["OkBtn"]);
            }
            finally
            {
                MainWindow.IsAuthenticating = false;
            }
        }
    }
}