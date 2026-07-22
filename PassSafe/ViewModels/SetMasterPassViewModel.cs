namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging;
    using Microsoft.Maui.ApplicationModel;
    using PassSafe.Messages;
    using PassSafe.Services;
    using System;
    using System.Threading.Tasks;

    public partial class SetMasterPassViewModel : ObservableObject, IRecipient<DatabaseImportedMessage>
    {
        [ObservableProperty]
        private bool areConditionsMet;

        [ObservableProperty]
        private string masterPass;

        [ObservableProperty]
        private string masterPassRepeat;

        [ObservableProperty]
        private string errorMessage;

        [ObservableProperty]
        private string securityQuestion;

        [ObservableProperty]
        private string securityQuestionAnswer;

        private readonly IDialogService _dialogService;
        private readonly SettingsViewModel _svm;
        private readonly SafeViewModel _sfvm;

        public SetMasterPassViewModel(IDialogService dialogService, SettingsViewModel settingsViewModel, SafeViewModel safeViewModel)
        {
            _dialogService = dialogService;
            _svm = settingsViewModel;
            _sfvm = safeViewModel;

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        partial void OnMasterPassChanged(string value) => CheckConditions();
        partial void OnMasterPassRepeatChanged(string value) => CheckConditions();
        partial void OnSecurityQuestionAnswerChanged(string value) => CheckConditions();
        partial void OnSecurityQuestionChanged(string value) => CheckConditions();

        private void CheckConditions()
        {
            bool isPasswordValid = false;

            if (string.IsNullOrEmpty(MasterPass) || string.IsNullOrEmpty(MasterPassRepeat))
            {
                ErrorMessage = string.Empty;
            }
            else if (MasterPass != MasterPassRepeat)
            {
                ErrorMessage = "Şifreler uyuşmuyor!";
            }
            else if (MasterPass.Length < 4)
            {
                ErrorMessage = "Şifre çok kısa!";
            }
            else
            {
                ErrorMessage = string.Empty;
                isPasswordValid = true;
            }

            bool isQuestionValid = !string.IsNullOrEmpty(SecurityQuestion);
            bool isAnswerValid = !string.IsNullOrWhiteSpace(SecurityQuestionAnswer);

            AreConditionsMet = isPasswordValid && isQuestionValid && isAnswerValid;
        }

        [RelayCommand]
        private async Task SetMasterPass()
        {
            try
            {
                var result = await _dialogService.ShowConfirmAsync("Emin misiniz?", "Eğer güvenlik sorusunun cevabını unutursanız şifrenizi sıfırlayamaz ve kasanızı tamamen kaybedersiniz!", "Evet, eminim", "İptal");
                if (result == true)
                {
                    await SecureStorage.SetAsync("masterPass", MasterPass);
                    await SecureStorage.SetAsync("securityQuestion", SecurityQuestion);
                    await SecureStorage.SetAsync("securityQuestionAnswer", SecurityQuestionAnswer);

                    await _dialogService.ShowAlertAsync("Hoş Geldiniz!", "Ana şifreniz başarıyla oluşturuldu. Kasanız kullanıma hazır.", "Tamam");
                    await Mopups.Services.MopupService.Instance.PopAsync();
                }
            }
            catch (Exception)
            {
                await _dialogService.ShowAlertAsync("Hata", "Şifre kaydedilirken bir sorun oluştu.", "Tamam");
            }
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
        }
    }
}