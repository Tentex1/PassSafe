namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using PassSafe.Helpers;
    using Plugin.Maui.Biometric;
    using System;
    using System.Security.Authentication;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Maui.Controls;

    /// <summary>
    /// Serves as the Entry Point of the App. Handles biometric security and master password verification on launch.
    /// </summary>
    public partial class MainViewModel(SafeViewModel sfvm, IDialogService dialogService, INavigationService navigationService, IBiometric biometricService, IDatabaseService databaseService) : ObservableObject
    {
        public LocalizationManager Loc => LocalizationManager.Instance;

        [RelayCommand]
        private async Task InitializeAsync()
        {
            if (MainWindow.IsAuthenticating)
                return;

            bool isAuthenticated = await AuthenticateAsync();

            if (isAuthenticated)
            {
                await CheckMasterPassAsync();
            }
        }

        /// <summary>
        /// Triggers the device's native biometric (Fingerprint/FaceID) or PIN authentication.
        /// </summary>
        private async Task<bool> AuthenticateAsync()
        {
            MainWindow.IsAuthenticating = true;

            try
            {
                var biometricStatus = await biometricService.GetAuthenticationStatusAsync();

                if (biometricStatus == BiometricHwStatus.NotEnrolled)
                {
                    await dialogService.ShowErrorAsync(new AuthenticationException(Loc["Error_NoPasscodeOrBiometricsSet"]));
                    CloseApplication();
                    return false;
                }

                AuthenticationRequest ar = new AuthenticationRequest
                {
                    Title = Loc["AuthTitle"],
                    Description = Loc["AuthDesc"],
                    AuthStrength = AuthenticatorStrength.Strong,
                    AllowPasswordAuth = biometricStatus != BiometricHwStatus.Success
                };

                var authresponse = await biometricService.AuthenticateAsync(ar, CancellationToken.None);

                if (authresponse.Status == BiometricResponseStatus.Success)
                {
                    return true;
                }
                else
                {
                    await dialogService.ShowErrorAsync(new AuthenticationException(Loc["AuthFailed"]));
                    CloseApplication();
                    return false;
                }
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync(ex);
                CloseApplication();
                return false;
            }
            finally
            {
                MainWindow.IsAuthenticating = false;
            }
        }

        /// <summary>
        /// Checks if a master password exists on the device. If not, forces the user to create one.
        /// </summary>
        private async Task CheckMasterPassAsync()
        {
            try
            {
                string masterPass = await SecureStorage.GetAsync("masterPass");

                if (!string.IsNullOrEmpty(masterPass))
                {
                    await databaseService.InitializeDatabaseAsync(masterPass);
                    await sfvm.LoadPasswordsCommand.ExecuteAsync(null);
                }
                else
                {
                    MainWindow.IsAuthenticating = true;

                    var result = await dialogService.ShowConfirmAsync(Loc["LoginTitle"], Loc["NoMasterPassFound"], Loc["SetBtn"], Loc["CancelBtn"]);
                    if (result == true)
                    {
                        await dialogService.ShowPopupAsync(new Views.SetMasterPassPopup());

                        masterPass = await SecureStorage.GetAsync("masterPass");

                        if (!string.IsNullOrEmpty(masterPass))
                        {
                            await databaseService.InitializeDatabaseAsync(masterPass);
                            await sfvm.LoadPasswordsCommand.ExecuteAsync(null);
                        }
                    }
                    else
                    {
                        CloseApplication();
                    }
                }
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync(ex);
            }
            finally
            {
                MainWindow.IsAuthenticating = false;
            }
        }

        private void CloseApplication()
        {
#if ANDROID
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#else
            Application.Current?.Quit();
#endif
        }
    }
}