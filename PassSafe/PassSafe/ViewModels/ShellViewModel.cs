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

        private readonly object _safeView;
        private readonly object _passGeneratorView;
        private readonly object _passAnalyzerView;
        private readonly object _settingsView;

        public IDialogService _dialogService;

        public IBiometric _biometricService;

        public ShellViewModel(IDialogService dialogService, IBiometric biometricService, SafeView safeView,
                          PassGeneratorView passGeneratorView,
                          PassAnalyzerView passAnalyzerView,
                          SettingsView settingsView)
        {
            _dialogService = dialogService;
            _biometricService = biometricService;
            _safeView = safeView;
            _passGeneratorView = passGeneratorView;
            _passAnalyzerView = passAnalyzerView;
            _settingsView = settingsView;
            CurrentView = _safeView;
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
                    await _dialogService.ShowErrorAsync("Hata", new Exception(),"Kopyala", "Tamam");
                    await Task.Delay(1000);
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
                "vault" => _safeView,
                "passgenerator" => _passGeneratorView,
                "passanalyzer" => _passAnalyzerView,
                "settings" => _settingsView,
                _ => _safeView
            };
        }
    }
}
