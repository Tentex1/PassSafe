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

        // RAM Tasarrufu için Readonly Diziler
        private readonly char[] _alphabetUpper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private readonly char[] _alphabetLower = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private readonly char[] _digits = "0123456789".ToCharArray();
        private readonly char[] _passwordSymbols = "!@#$%^&*()-_+=".ToCharArray();

        public PassGeneratorViewModel()
        {
            GeneratePassword();
        }

        // Anahtarlar kapatılıp/açıldığında Şifreyi Anında Yenile!
        partial void OnIsUseUpperLetterChanged(bool value) => GeneratePassword();
        partial void OnIsUseLowerLetterChanged(bool value) => GeneratePassword();
        partial void OnIsUseNumbersChanged(bool value) => GeneratePassword();
        partial void OnIsUseSymbolsChanged(bool value) => GeneratePassword();
        partial void OnGeneratedPassLengthChanged(int value) => GeneratePassword();

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
                GeneratedPass = "Seçim Yapın!";
                return;
            }

            char[] chosenChars = Random.Shared.GetItems(dynamicPool.ToArray(), GeneratedPassLength);
            GeneratedPass = new string(chosenChars);
        }

        [RelayCommand]
        private async Task CopyPassword()
        {
            await Clipboard.Default.SetTextAsync(GeneratedPass);
            await Toast.Make("Şifre kopyalandı!").Show();
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