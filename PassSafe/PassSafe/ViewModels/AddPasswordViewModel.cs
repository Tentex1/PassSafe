namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging;
    using Microsoft.Maui.Graphics;
    using PassSafe.Messages;
    using PassSafe.Models;
    using PassSafe.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class AddPasswordViewModel : ObservableObject
    {
        IDatabaseService _databaseService;

        ICryptoService _cryptoService;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string userName = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SecurityProgress))]
        [NotifyPropertyChangedFor(nameof(SecurityStatus))]
        [NotifyPropertyChangedFor(nameof(SecurityColor))]
        private string password = string.Empty;

        private readonly List<string> _icons = new List<string>
        {
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Vpn_key,
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Lock,
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Fingerprint,
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Shield,
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Account_circle,

            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Group,       
    
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Credit_card,
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Payments,

            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Currency_bitcoin,
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Shopping_bag,

            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Account_balance,
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Mail,
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Forum,
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Public,
    
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Sports_esports,        
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Tv,      
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Work,        
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.School,       
            UraniumUI.Icons.MaterialSymbols.MaterialSharp.Cloud        
        };

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentIcon))]
        private int _currentIconIndex = 0;

        public string CurrentIcon => _icons[CurrentIconIndex];

        [RelayCommand(CanExecute = nameof(CanNext))]
        private void Next()
        {
            CurrentIconIndex++;
            NextCommand.NotifyCanExecuteChanged();
            PreviousCommand.NotifyCanExecuteChanged();
        }

        private bool CanNext() => CurrentIconIndex < _icons.Count - 1;

        [RelayCommand(CanExecute = nameof(CanPrevious))]
        private void Previous()
        {
            CurrentIconIndex--;
            NextCommand.NotifyCanExecuteChanged();
            PreviousCommand.NotifyCanExecuteChanged();
        }

        private bool CanPrevious() => CurrentIconIndex > 0;

        public double SecurityProgress => (double)CalculatePasswordScore() / 5;

        public string SecurityStatus => CalculatePasswordScore() switch
        {
            0 => "Empty",
            1 => "Very Weak",
            2 => "Weak",
            3 => "Medium",
            4 => "Strong",
            5 => "Impossible",
            _ => "Unknown"
        };

        public Color SecurityColor => CalculatePasswordScore() switch
        {
            0 => Colors.Gray,
            1 => Colors.Red,
            2 => Colors.Orange,
            3 => Colors.Yellow,
            4 => Colors.LightGreen,
            5 => Colors.Green,
            _ => Colors.Gray
        };

        public AddPasswordViewModel(IDatabaseService databaseService, ICryptoService cryptoService)
        {
            _databaseService = databaseService;
            _cryptoService = cryptoService;
        }

        private int CalculatePasswordScore()
        {
            if (string.IsNullOrEmpty(Password)) return 0;

            int score = 0;
            if (Password.Length >= 8) score++;
            if (Password.Any(char.IsUpper)) score++;
            if (Password.Any(char.IsLower)) score++;
            if (Password.Any(char.IsDigit)) score++;
            if (Password.Any(ch => char.IsPunctuation(ch) || char.IsSymbol(ch))) score++;

            return score;
        }

        [RelayCommand]
        private async Task AddPassword()
        {
            var data = new Password()
            {
                Title = Title,
                UserName = UserName,
                Icon = CurrentIcon,
                SecurityProgress = SecurityProgress,
                SecurityStatus = SecurityStatus,
                EncryptedPassword = _cryptoService.Encrypt(Password, await SecureStorage.GetAsync("master_pass"))
            };

            await _databaseService.AddPassword(data);

            Password = string.Empty;
            UserName = string.Empty;
            Title = string.Empty;
            CurrentIconIndex = 0;

            NextCommand.NotifyCanExecuteChanged();
            PreviousCommand.NotifyCanExecuteChanged();

            await Mopups.Services.MopupService.Instance.PopAsync();
        }
    }
}
