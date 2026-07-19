
using PassSafe.Messages;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace PassSafe.ViewModels
{
    public partial class ImportDatabaseVerifyViewModel(IDatabaseService databaseService, IDialogService dialogService) : ObservableObject
    {
        [ObservableProperty]
        private bool isVerified;

        [ObservableProperty]
        private bool isButtonEnabled = true;

        [ObservableProperty]
        private string masterPass;

        [ObservableProperty]
        private string infoText = "Şifre girin.";

        [RelayCommand]
        private async Task VerifyAsync()
        {
            IsButtonEnabled = true;
            try
            {
                var result = await Task.Run(() => databaseService.InitializeDatabaseAsync(MasterPass));

                if (result == true)
                {
                    InfoText = "Başarılı!";
                    await Task.Delay(1500);
                    IsVerified = true;
                    await SecureStorage.SetAsync("master_pass", MasterPass);
                    await Mopups.Services.MopupService.Instance.PopAsync();
                    WeakReferenceMessenger.Default.Send(new DatabaseImportedMessage());
                }
                else
                {
                    InfoText = "Parola yanlış!";
                    IsVerified = false;
                }
            }
            catch (Exception ex)
            {
                await dialogService.ShowErrorAsync(ex);
            }
        }
    }
}
