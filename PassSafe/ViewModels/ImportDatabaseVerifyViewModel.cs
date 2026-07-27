namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging;
    using PassSafe.Helpers;
    using PassSafe.Messages;
    using PassSafe.Services;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Handles the verification process when the user imports an old SQLite backup.
    /// Ensures they know the old master password before fully restoring.
    /// </summary>
    public partial class ImportDatabaseVerifyViewModel : ObservableObject
    {
        public LocalizationManager Loc => LocalizationManager.Instance;

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
        private string infoText;

        [ObservableProperty]
        private Color infoTextColor = Colors.Gray;

        public ImportDatabaseVerifyViewModel(IDatabaseService databaseService, IDialogService dialogService)
        {
            _databaseService = databaseService;
            _dialogService = dialogService;
            InfoText = Loc["InfoEnterOldVaultDetails"];
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

        /// <summary>
        /// Tries to initialize the imported SQLite database using the provided password.
        /// Reverts changes if authentication fails.
        /// </summary>
        [RelayCommand]
        private async Task VerifyAsync()
        {
            IsButtonEnabled = false;
            try
            {
                var result = await Task.Run(() => _databaseService.InitializeDatabaseAsync(MasterPass));

                if (result)
                {
                    InfoTextColor = Colors.Green;
                    InfoText = Loc["InfoPassVerifiedLoad"];

                    await SecureStorage.SetAsync("masterPass", MasterPass);
                    await SecureStorage.SetAsync("securityQuestion", SecurityQuestion);
                    await SecureStorage.SetAsync("securityQuestionAnswer", SecurityQuestionAnswer);

                    IsVerified = true;
                    await Task.Delay(1000);

                    await Mopups.Services.MopupService.Instance.PopAsync();

                    // Tell the SafeViewModel to refresh its list from the newly imported database
                    WeakReferenceMessenger.Default.Send(new DatabaseImportedMessage());
                }
                else
                {
                    InfoTextColor = Colors.Red;
                    InfoText = Loc["InfoPassWrong"];
                    IsVerified = false;
                    CheckConditions();
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