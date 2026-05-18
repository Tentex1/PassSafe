namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using Microsoft.Maui.ApplicationModel;
    using PassSafe.Models;
    using PassSafe.Services;
    using PassSafe.Views;
    using Plugin.Maui.Biometric;
    using System;
    using System.Collections.ObjectModel;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="VaultViewModel" />
    /// </summary>
    public partial class VaultViewModel : ObservableObject
    {
        string master_pass;

        public IBiometric _biometricService;

        public IDialogService _dialogService;

        public IDatabaseService _databaseService;

        public ICryptoService _cryptoService;

        [ObservableProperty]
        private ObservableCollection<Password> passwords;

        [ObservableProperty]
        private string password;

        public VaultViewModel(ICryptoService cryptoService,IBiometric biometric, IDialogService dialogService, IDatabaseService databaseService)
        {
            _biometricService = biometric;
            _dialogService = dialogService;
            _databaseService = databaseService;
            _cryptoService = cryptoService;

            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await Initialize();
                Password password = new()
                {
                    Title = "Google",
                    UserName = "sdlkjfhdsjkl@gmail.com",
                    EncryptedPassword = _cryptoService.Encrypt(Password, master_pass)
                };
                await _dialogService.ShowAlertAsync(password.EncryptedPassword,password.EncryptedPassword,"tm");
                await _databaseService.AddPassword(password);
                Passwords = new(await _databaseService.GetDatabase());
            });
        }

        private async Task Initialize()
        {
            await Task.Delay(500);

            bool isAuthenticated = await AuthenticateAsync();

            if (isAuthenticated)
            {
                await CheckMasterPass();
            }
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

        private async Task CheckMasterPass()
        {
            try
            {
                master_pass = await SecureStorage.GetAsync("master_pass");

                if (master_pass != null)
                {
                    await _databaseService.InitializeDatabase(master_pass);
                }
                else
                {
                    var result = await _dialogService.ShowConfirmAsync("Hata", "Ana şifre belirlememişsiniz", "Belirle", "İptal");
                    if (result == true)
                    {

                        _dialogService.ShowPopup(new SetMasterPassPopup());
                    }
                    else
                    {
                        Application.Current?.Quit();
                    }
                }
            }
            catch (Exception ex) { await _dialogService.ShowAlertAsync("Error", ex.Message, "Copy"); }
        }
    }
}
