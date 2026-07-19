using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassSafe.Messages;
using PassSafe.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PassSafe.ViewModels
{
    public partial class SetMasterPassViewModel : ObservableObject, IRecipient<DatabaseImportedMessage>
    {
        [ObservableProperty]
        private string masterPass;

        public IDialogService _dialogService;
        public SettingsViewModel _svm;
        public SafeViewModel _sfvm;
        public ImportDatabaseVerifyViewModel _idvvm;

        public SetMasterPassViewModel(IDialogService dialogService, SettingsViewModel settingsViewModel, ImportDatabaseVerifyViewModel importDatabaseVerifyViewModel, SafeViewModel safeViewModel)
        {
            _dialogService = dialogService;
            _svm = settingsViewModel;
            _sfvm = safeViewModel;
            _idvvm = importDatabaseVerifyViewModel;

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        [RelayCommand]
        private async Task SetMasterPass()
        {
            try
            {
                var result = await _dialogService.ShowConfirmAsync("Are you sure?", "If you forget this password, you won't be able to change it again!", "OK", "Cancel");
                if(result == true)
                {
                    await SecureStorage.SetAsync("master_pass", MasterPass);
                    await _dialogService.ShowAlertAsync("OK!", "Master pass setted succesfully", "OK");
                }
            }
            catch(Exception ex) { await _dialogService.ShowConfirmAsync("Error", "'master_pass' could not be set.", "Copy Error Code", "OK"); }
        }

        [RelayCommand]
        private async Task ImportOldDatabaseAsync()
        {
            await _svm.ImportDatabaseCommand.ExecuteAsync(null);
            
            //var result = _svm.IsImportSuccessfull;

            //if (result == true)
            //{
            //    await _dialogService.ShowPopupAsync(new ImportDatabaseVerifyPopup());

            //    if (_idvvm.IsVerified == true)
            //    {
            //        await _dialogService.ShowAlertAsync("Başarılı!", "Veritabanınız başarıyla içe aktarıldı.", "Tamam");
            //    }
            //    else
            //    {
            //        await _dialogService.ShowAlertAsync("Başarısız!", "Veritabanının şifresi yanlış.", "Tamam");
            //    }
            //}
            //else
            //{
            //    await _dialogService.ShowAlertAsync("Başarısız!", "Veritabanı dosyanız bozuk veya yanlış, SQLite veritabanı seçin.", "Tamam");
            //}
        }

        public async void Receive(DatabaseImportedMessage message)
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
            await Mopups.Services.MopupService.Instance.PopAsync();
            await _sfvm.LoadPasswordsCommand.ExecuteAsync(null);
            await _sfvm.LoadFavoritesCommand.ExecuteAsync(null);
        }
    }
}
