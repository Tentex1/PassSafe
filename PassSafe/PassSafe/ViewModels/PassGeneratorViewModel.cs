namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using PassSafe.Helpers;
    using PassSafe.Views;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="PassGeneratorViewModel" />
    /// </summary>
    public partial class PassGeneratorViewModel : ObservableObject
    {
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

        List<char> alphabetUpper = new List<char>
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
        };

        List<char> alphabetLower = new List<char>
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
        };

        List<char> digits = new List<char>
        {
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'
        };

        List<char> passwordSymbols = new List<char>
        {
            '!', '@', '#', '$', '%', '^', '&', '*', '(', ')', '-', '_', '+', '='
        };


        public PassGeneratorViewModel()
        {
            GeneratePasswordCommand.Execute(null);
        }

        [RelayCommand]
        private void GeneratePassword()
        {
            List<char> dynamicPool = new List<char>();

            if (IsUseUpperLetter) dynamicPool.AddRange(alphabetUpper);
            if (IsUseLowerLetter) dynamicPool.AddRange(alphabetLower);
            if (IsUseNumbers) dynamicPool.AddRange(digits);
            if (IsUseSymbols) dynamicPool.AddRange(passwordSymbols);

            if (dynamicPool.Count == 0)
            {
                GeneratedPass = "Lütfen en az bir seçenek seçin!";
                return;
            }

            int passwordLength = GeneratedPassLength;

            char[] chosenChars = Random.Shared.GetItems(dynamicPool.ToArray(), passwordLength);

            GeneratedPass = new string(chosenChars);
        }

        [RelayCommand]
        private async Task CopyPassword()
        {
            await Clipboard.Default.SetTextAsync(GeneratedPass);
        }

        [RelayCommand]
        private async Task AddPasswordToSafeAsync()
        {
            var vm = App.Services.GetService<AddPasswordViewModel>();
            vm.Password = GeneratedPass;
            await Mopups.Services.MopupService.Instance.PushAsync(new AddPasswordPopup(vm));
        }
    }
}
