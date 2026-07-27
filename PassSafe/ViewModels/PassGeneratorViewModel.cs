namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Maui.Alerts;
    using Microsoft.Maui.ApplicationModel.DataTransfer;
    using PassSafe.Helpers;
    using PassSafe.Views;
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Manages the Password Generator page. Generates secure random strings based on user settings.
    /// </summary>
    public partial class PassGeneratorViewModel : ObservableObject
    {
        public LocalizationManager Loc => LocalizationManager.Instance;

        [ObservableProperty]
        private string generatedPass;

        [ObservableProperty]
        private bool isUseUpperLetter = true;

        [ObservableProperty]
        private bool isUseLowerLetter = true;

        [ObservableProperty]
        private bool isUseNumbers = true;

        [ObservableProperty]
        private bool isUseSymbols = true;

        [ObservableProperty]
        private int generatedPassLength = 8;

        // Using ReadOnly char arrays to optimize RAM usage
        private readonly char[] _alphabetUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private readonly char[] _alphabetLower = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private readonly char[] _digits = "0123456789".ToCharArray();
        private readonly char[] _passwordSymbols = "!@#$%^&*()-_+=".ToCharArray();

        public PassGeneratorViewModel()
        {
            GeneratePassword();
        }

        // Live generation: Generates a new password instantly when any setting changes.
        internal partial void OnIsUseUpperLetterChanged(bool value) => GeneratePassword();
        internal partial void OnIsUseLowerLetterChanged(bool value) => GeneratePassword();
        internal partial void OnIsUseNumbersChanged(bool value) => GeneratePassword();
        internal partial void OnIsUseSymbolsChanged(bool value) => GeneratePassword();
        internal partial void OnGeneratedPassLengthChanged(int value) => GeneratePassword();

        /// <summary>
        /// Creates a random secure password based on the selected character pools.
        /// </summary>
        [RelayCommand]
        private void GeneratePassword()
        {
            List<char> dynamicPool = new List<char>();

            if (IsUseUpperLetter) dynamicPool.AddRange(_alphabetUpper);
            if (IsUseLowerLetter) dynamicPool.AddRange(_alphabetLower);
            if (IsUseNumbers) dynamicPool.AddRange(_digits);
            if (IsUseSymbols) dynamicPool.AddRange(_passwordSymbols);

            if (dynamicPool.Count == 0)
            {
                GeneratedPass = Loc["GenSelectError"];
                return;
            }

            char[] chosenChars = Random.Shared.GetItems(dynamicPool.ToArray(), GeneratedPassLength);
            GeneratedPass = new string(chosenChars);
        }

        [RelayCommand]
        private async Task CopyPassword()
        {
            await Clipboard.Default.SetTextAsync(GeneratedPass);
            await Toast.Make(Loc["MsgCopied"]).Show();
        }

        /// <summary>
        /// Opens the Add Password popup and pre-fills it with the generated password.
        /// </summary>
        [RelayCommand]
        private async Task AddPasswordToSafeAsync()
        {
            var vm = App.Services.GetService<AddPasswordViewModel>();
            vm.Password = GeneratedPass;
            await Mopups.Services.MopupService.Instance.PushAsync(new AddPasswordPopup(vm));
        }
    }
}