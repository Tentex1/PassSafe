namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging;
    using PassSafe.Messages;
    using PassSafe.Services;
    using PassSafe.Views;
    using Plugin.Maui.Biometric;
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public partial class ShellViewModel : ObservableObject
    {
        [ObservableProperty]
        private object currentView;

        private readonly SafeView safeView = new();

        private readonly PassGeneratorView passGeneratorView = new();

        private readonly PassAnalyzerView passAnalyzerView = new();

        private readonly SettingsView settingsView = new();

        public IDialogService _dialogService;

        public IBiometric _biometricService;

        public ShellViewModel(IDialogService dialogService, IBiometric biometricService)
        {
            CurrentView = safeView;
            _dialogService = dialogService;
            _biometricService = biometricService;
        }

        [RelayCommand]
        private async Task InitializeAsync()
        {
            await AuthenticateAsync();
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

                var authresponse = await _biometricService.AuthenticateAsync(ar, CancellationToken.None);

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
                "vault" => safeView,
                "passgenerator" => passGeneratorView,
                "passanalyzer" => passAnalyzerView,
                "settings" => settingsView,
                _ => safeView
            };
        }
    }
}
