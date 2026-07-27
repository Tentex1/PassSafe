namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging;
    using PassSafe.Helpers;
    using PassSafe.Messages;
    using PassSafe.Services;
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    /// <summary>
    /// Manages the popup for creating a new Vault (Master Password and Security Questions).
    /// </summary>
    public partial class SetMasterPassViewModel : ObservableObject, IRecipient<DatabaseImportedMessage>
    {
        public LocalizationManager Loc => LocalizationManager.Instance;

        [ObservableProperty]
        private ObservableCollection<string> securityQuestions;

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

        /// <summary>
        /// Initializes services and populates the security questions list.
        /// </summary>
        public SetMasterPassViewModel(IDialogService dialogService, SettingsViewModel settingsViewModel, SafeViewModel safeViewModel)
        {
            _dialogService = dialogService;
            _svm = settingsViewModel;
            _sfvm = safeViewModel;

            SecurityQuestions = new ObservableCollection<string>() {
                Loc["FirstSecurityQuestion"],
                Loc["SecondSecurityQuestion"],
                Loc["ThirdSecurityQuestion"],
                Loc["FourthSecurityQuestion"],
                Loc["FifthSecurityQuestion"]
            };

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        // Live validation checks when user types
        partial void OnMasterPassChanged(string value) => CheckConditions();
        partial void OnMasterPassRepeatChanged(string value) => CheckConditions();
        partial void OnSecurityQuestionAnswerChanged(string value) => CheckConditions();
        partial void OnSecurityQuestionChanged(string value) => CheckConditions();

        /// <summary>
        /// Validates user inputs. Updates error messages and enables/disables the Save button.
        /// </summary>
        private void CheckConditions()
        {
            bool isPasswordValid = false;

            if (string.IsNullOrEmpty(MasterPass) || string.IsNullOrEmpty(MasterPassRepeat))
            {
                ErrorMessage = string.Empty;
            }
            else if (MasterPass != MasterPassRepeat)
            {
                ErrorMessage = Loc["ErrorPasswordsNotMatch"];
            }
            else if (MasterPass.Length < 4)
            {
                ErrorMessage = Loc["ErrorPasswordTooShort"];
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

        /// <summary>
        /// Saves the Master Password and Security Answers securely to the device.
        /// </summary>
        [RelayCommand]
        private async Task SetMasterPass()
        {
            try
            {
                var result = await _dialogService.ShowConfirmAsync(Loc["AreYouSureTitle"], Loc["WarningMasterPassForget"], Loc["YesImSureBtn"], Loc["CancelBtn"]);
                if (result == true)
                {
                    await SecureStorage.SetAsync("masterPass", MasterPass);
                    await SecureStorage.SetAsync("securityQuestion", SecurityQuestion);
                    await SecureStorage.SetAsync("securityQuestionAnswer", SecurityQuestionAnswer);

                    await _dialogService.ShowAlertAsync(Loc["WelcomeTitle"], Loc["SuccessVaultCreated"], Loc["OkBtn"]);
                    await Mopups.Services.MopupService.Instance.PopAsync();
                }
            }
            catch (Exception)
            {
                await _dialogService.ShowAlertAsync(Loc["ErrorTitle"], Loc["ErrorPassSaveFailed"], Loc["OkBtn"]);
            }
        }

        /// <summary>
        /// Routes the user to the Import Database flow inside SettingsViewModel.
        /// </summary>
        [RelayCommand]
        private async Task ImportOldDatabaseAsync()
        {
            await _svm.ImportDatabaseCommand.ExecuteAsync(null);
        }

        /// <summary>
        /// Closes the popup and refreshes the vault when an old database is successfully imported.
        /// </summary>
        public async void Receive(DatabaseImportedMessage message)
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
            await Mopups.Services.MopupService.Instance.PopAsync();
            await _sfvm.LoadPasswordsCommand.ExecuteAsync(null);
        }
    }
}