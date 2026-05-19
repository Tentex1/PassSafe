namespace PassSafe.ViewModels
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging; // Messenger için ekledik
    using PassSafe.Messages;                // Yukarıdaki mesaj sınıfı için ekledik
    using PassSafe.Services;
    using PassSafe.Views;
    using Plugin.Maui.Biometric;

    /// <summary>
    /// Defines the <see cref="ShellViewModel" />
    /// </summary>
    public partial class ShellViewModel : ObservableObject
    {
        [ObservableProperty]
        private object currentView;

        private readonly VaultView vaultView = new();

        private readonly PassGeneratorView passGeneratorView = new();

        private readonly PassAnalyzerView passAnalyzerView = new();

        private readonly SettingsView settingsView = new();

        public IDialogService _dialogService;

        public IBiometric _biometricService;

        public ShellViewModel(IDialogService dialogService, IBiometric biometricService)
        {
            CurrentView = vaultView;
            _dialogService = dialogService;
            _biometricService = biometricService;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Task.Delay(500);
                await AuthenticateAsync();
            });
        }

        private async Task<bool> AuthenticateAsync()
        {
            try
            {
                AuthenticationRequest ar = new AuthenticationRequest
                {
                    Title = "Doğrulamayı tamamlayın.",
                    Description = "Şifrelerinize erişmek için doğrulamayı tamamlayın.",
                    AuthStrength = AuthenticatorStrength.Strong,
                    AllowPasswordAuth = true
                };

                // Biyometrik doğrulamayı senin kodundaki gibi çağırıyoruz
                var authresponse = await _biometricService.AuthenticateAsync(ar, CancellationToken.None);

                // ==========================================================
                // İŞTE MESAJI FIRLATTIĞIMIZ TEK SATIR:
                // ==========================================================
                WeakReferenceMessenger.Default.Send(new AuthResultMessage(authresponse));

                if (authresponse.Status == BiometricResponseStatus.Success)
                {
                    return true;
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Hata", "Kimlik doğrulama başarısız.", "Tamam");
                    Application.Current?.Quit();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Hata", $"{ex.Message}", "Tamam");
                return false;
            }
        }

        [RelayCommand]
        private void ChangeTab(string tabName)
        {
            CurrentView = tabName switch
            {
                "vault" => vaultView,
                "passgenerator" => passGeneratorView,
                "passanalyzer" => passAnalyzerView,
                "settings" => settingsView,
                _ => vaultView
            };
        }
    }
}