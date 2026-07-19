using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PassSafe.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace PassSafe.ViewModels
{
    public partial class SetMasterPassViewModel : ObservableObject
    {
        [ObservableProperty]
        private string masterPass;

        public IDialogService _dialogService;

        public SetMasterPassViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
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
                    Application.Current.Quit();
                }
                else { Application.Current.Quit(); }
            }
            catch(Exception ex) { await _dialogService.ShowConfirmAsync("Error", "'master_pass' could not be set.", "Copy Error Code", "OK"); }
        }
    }
}
