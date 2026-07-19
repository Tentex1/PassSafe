using Plugin.Maui.Biometric;
using System.Security.Authentication;

namespace PassSafe.ViewModels
{
    public partial class MainViewModel(SafeViewModel sfvm, IDialogService dialogService, INavigationService navigationService, IBiometric biometricService, IDatabaseService databaseService) : ObservableObject
    {

        [RelayCommand]
        private async Task InitializeAsync()
        {
            bool isAuthenticated = await AuthenticateAsync();

            if (isAuthenticated)
            {
                await CheckMasterPassAsync();
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

                var authresponse = await biometricService.AuthenticateAsync(ar, CancellationToken.None);

                if (authresponse.Status == BiometricResponseStatus.Success)
                {
                    return true;
                }
                else
                {
                    await dialogService.ShowErrorAsync(new AuthenticationException());
                    await Task.Delay(1000);
                    Application.Current?.Quit();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync(ex);
                return false;
            }
        }

        private async Task CheckMasterPassAsync()
        {
            try
            {
                string master_pass = await SecureStorage.GetAsync("master_pass");

                if (!string.IsNullOrEmpty(master_pass))
                {
                    await databaseService.InitializeDatabaseAsync(master_pass);
                    sfvm.IsRefreshing = true;
                }
                else
                {
                    var result = await dialogService.ShowConfirmAsync("Giriş", "Ana şifre belirlememişsiniz", "Belirle", "İptal");
                    if (result == true)
                    {
                        await dialogService.ShowPopupAsync(new SetMasterPassPopup());

                        master_pass = await SecureStorage.GetAsync("master_pass");

                        if (!string.IsNullOrEmpty(master_pass))
                        {
                            await databaseService.InitializeDatabaseAsync(master_pass);
                            sfvm.IsRefreshing = true;
                        }
                    }
                    else
                    {
#if ANDROID
                        Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#else
                        Application.Current?.Quit();
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync(ex);
            }
        }
    }
}
