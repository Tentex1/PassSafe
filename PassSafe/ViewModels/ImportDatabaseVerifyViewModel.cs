namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using PassSafe.Messages;
    using PassSafe.Services;
    using System;
    using System.Threading.Tasks;

    public partial class ImportDatabaseVerifyViewModel : ObservableObject
    {
        private readonly IDatabaseService _databaseService;
        private readonly IDialogService _dialogService;

        [ObservableProperty]
        private bool isVerified;

        [ObservableProperty]
        private bool isButtonEnabled;

        [ObservableProperty]
        private string masterPass;

        [ObservableProperty]
        private string securityQuestion;

        [ObservableProperty]
        private string securityQuestionAnswer;

        [ObservableProperty]
        private string infoText = "Şifre ve güvenlik bilgilerini girin.";

        [ObservableProperty]
        private Color infoTextColor = Colors.Gray;

        public ImportDatabaseVerifyViewModel(IDatabaseService databaseService, IDialogService dialogService)
        {
            _databaseService = databaseService;
            _dialogService = dialogService;
        }

        partial void OnMasterPassChanged(string value) => CheckConditions();
        partial void OnSecurityQuestionChanged(string value) => CheckConditions();
        partial void OnSecurityQuestionAnswerChanged(string value) => CheckConditions();

        private void CheckConditions()
        {
            bool isPasswordValid = !string.IsNullOrWhiteSpace(MasterPass);
            bool isQuestionValid = !string.IsNullOrWhiteSpace(SecurityQuestion);
            bool isAnswerValid = !string.IsNullOrWhiteSpace(SecurityQuestionAnswer);

            IsButtonEnabled = isPasswordValid && isQuestionValid && isAnswerValid;
        }

        [RelayCommand]
        private async Task VerifyAsync()
        {
            IsButtonEnabled = false;
            try
            {
                // 1. Veritabanını girilen şifre ile doğrulamayı dene
                var result = await Task.Run(() => _databaseService.InitializeDatabaseAsync(MasterPass));

                if (result)
                {
                    InfoTextColor = Colors.Green;
                    InfoText = "Şifre doğru! Bilgiler kaydediliyor...";

                    // 2. Şifreyi ve yeni Güvenlik Sorusu / Cevabını SecureStorage'a kaydet
                    await SecureStorage.SetAsync("masterPass", MasterPass);
                    await SecureStorage.SetAsync("securityQuestion", SecurityQuestion);
                    await SecureStorage.SetAsync("securityQuestionAnswer", SecurityQuestionAnswer);

                    IsVerified = true;
                    await Task.Delay(1000);

                    // 3. Popup'ı kapat ve mesaj fırlat
                    await Mopups.Services.MopupService.Instance.PopAsync();
                    WeakReferenceMessenger.Default.Send(new DatabaseImportedMessage());
                }
                else
                {
                    InfoTextColor = Colors.Red;
                    InfoText = "Parola yanlış! Lütfen tekrar deneyin.";
                    IsVerified = false;
                    CheckConditions(); // Butonu durumuna göre tekrar aktif et
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowErrorAsync(ex);
                CheckConditions();
            }
        }
    }
}