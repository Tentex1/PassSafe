namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using PassSafe.Messages;
    using PassSafe.Services;

    public partial class SetMasterPassViewModel : ObservableObject, IRecipient<DatabaseImportedMessage>
    {
        [ObservableProperty]
        private bool areConditionsMet;

        [ObservableProperty]
        private string masterPass;

        [ObservableProperty]
        private string masterPassRepeat;

        [ObservableProperty]
        private Color masterPassRepeatColor;

        [ObservableProperty]
        private string securityQuestion;

        [ObservableProperty]
        private string securityQuestionAnswer;

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

        partial void OnMasterPassChanged(string value)
        {
            CheckConditions();
        }

        partial void OnMasterPassRepeatChanged(string value)
        {
            CheckConditions();
        }

        partial void OnSecurityQuestionAnswerChanged(string value)
        {
            CheckConditions();
        }

        partial void OnSecurityQuestionChanged(string value)
        {
            CheckConditions();
        }

        private void CheckConditions()
        {
            if (string.IsNullOrEmpty(MasterPass) || string.IsNullOrEmpty(MasterPassRepeat))
            {
                MasterPassRepeatColor = Application.Current.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
            }
            else if (MasterPass == MasterPassRepeat)
            {
                MasterPassRepeatColor = Application.Current.RequestedTheme == AppTheme.Dark ? Colors.White : Colors.Black;
            }
            else
            {
                MasterPassRepeatColor = Color.FromRgb(255, 0, 0);
            }

            bool isPasswordValid = !string.IsNullOrEmpty(MasterPass) && MasterPass == MasterPassRepeat;
            bool isQuestionValid = !string.IsNullOrEmpty(SecurityQuestion);
            bool isAnswerValid = !string.IsNullOrWhiteSpace(SecurityQuestionAnswer);

            AreConditionsMet = isPasswordValid && isQuestionValid && isAnswerValid;
        }

        [RelayCommand]
        private async Task SetMasterPass()
        {
            try
            {
                var result = await _dialogService.ShowConfirmAsync("Emin misin??", "Eğer güvenlik sorusunun cevabını unutursan bi daha şifreni değiştiremezsin ve şifrelerini kaybedersin!", "Tamam", "Değiştireceğim");
                if (result == true)
                {
                    await SecureStorage.SetAsync("masterPass", MasterPass);
                    await SecureStorage.SetAsync("securityQuestion", SecurityQuestion);
                    await SecureStorage.SetAsync("securityQuestionAnswer", SecurityQuestionAnswer);
                    await _dialogService.ShowAlertAsync("Hoşgeldiniz!", "Ana şifre başarıyla ayarlandı!", "Tamam");
                    await Mopups.Services.MopupService.Instance.PopAsync();
                }
            }
            catch (Exception ex) { await _dialogService.ShowConfirmAsync("Error", "'masterPass' could not be set.", "Copy Error Code", "OK"); }
        }

        [RelayCommand]
        private async Task ImportOldDatabaseAsync()
        {
            await _svm.ImportDatabaseCommand.ExecuteAsync(null);
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
