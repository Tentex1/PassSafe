namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging;
    using MauiIcons.Core;
    using MauiIcons.Material.Sharp;
    using Microsoft.Maui.Graphics;
    using PassSafe.Messages;
    using PassSafe.Models;
    using PassSafe.Services;
    using PassSafe.Views;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class AddPasswordViewModel : ObservableObject
    {
        IDatabaseService _databaseService;

        ICryptoService _cryptoService;

        IDialogService _dialogService;

        SafeViewModel _sfvm;

        [ObservableProperty]
        private string title = string.Empty;

        [ObservableProperty]
        private string userName = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SecurityProgress))]
        [NotifyPropertyChangedFor(nameof(SecurityStatus))]
        [NotifyPropertyChangedFor(nameof(SecurityColor))]
        private string password = string.Empty;

        private readonly List<MauiIcon> _icons = new List<MauiIcon>
        {
            new MauiIcon() { Icon = MaterialSharpIcons.VpnKey },
            new MauiIcon() { Icon = MaterialSharpIcons.Lock },
            new MauiIcon() { Icon = MaterialSharpIcons.Fingerprint },
            new MauiIcon() { Icon = MaterialSharpIcons.Shield },
            new MauiIcon() { Icon = MaterialSharpIcons.AccountCircle },
            
            new MauiIcon() { Icon = MaterialSharpIcons.Group },
            
            new MauiIcon() { Icon = MaterialSharpIcons.CreditCard },
            new MauiIcon() { Icon = MaterialSharpIcons.Payments },
            new MauiIcon() { Icon = MaterialSharpIcons.CurrencyBitcoin },
            new MauiIcon() { Icon = MaterialSharpIcons.ShoppingBag },
            
            new MauiIcon() { Icon = MaterialSharpIcons.AccountBalance },
            new MauiIcon() { Icon = MaterialSharpIcons.Mail },
            new MauiIcon() { Icon = MaterialSharpIcons.Forum },
            new MauiIcon() { Icon = MaterialSharpIcons.Public },
            
            new MauiIcon() { Icon = MaterialSharpIcons.SportsEsports },
            new MauiIcon() { Icon = MaterialSharpIcons.Tv },
            new MauiIcon() { Icon = MaterialSharpIcons.Work },
            new MauiIcon() { Icon = MaterialSharpIcons.School },
            new MauiIcon() { Icon = MaterialSharpIcons.Cloud }
        };

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrentIcon))]
        private int _currentIconIndex = 0;

        public MauiIcon CurrentIcon => _icons[CurrentIconIndex];

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

        public AddPasswordViewModel(IDatabaseService databaseService, ICryptoService cryptoService, IDialogService dialogService, SafeViewModel sfvm)
        {
            _databaseService = databaseService;
            _cryptoService = cryptoService;
            _dialogService = dialogService;
            _sfvm = sfvm;

            Mopups.Services.MopupService.Instance.Popped += (s,e) =>
            {
                Password = string.Empty;
                UserName = string.Empty;
                Title = string.Empty;
                CurrentIconIndex = 0;

                NextCommand.NotifyCanExecuteChanged();
                PreviousCommand.NotifyCanExecuteChanged();
            };
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
        private async Task AddPasswordAsync()
        {
            var masterPass = await SecureStorage.GetAsync("master_pass");

            if (string.IsNullOrEmpty(masterPass))
            {
                var result = await _dialogService.ShowConfirmAsync("Hata", "Master key ayarlanmamış", "ayarla", "ayarlama");
                if(result == true)
                {
                    await _dialogService.ShowPopup(new SetMasterPassPopup());
                }
            }

            var encrypted = _cryptoService.Encrypt(Password, masterPass);

            var data = new Password()
            {
                Title = Title,
                UserName = UserName,
                Icon = CurrentIcon.Icon.ToString(),
                SecurityProgress = SecurityProgress,
                SecurityStatus = SecurityStatus,
                EncryptedPassword = encrypted   
            };

            await _databaseService.AddPassword(data);

            await _sfvm.LoadPasswordsCommand.ExecuteAsync(null);

            if (Mopups.Services.MopupService.Instance.PopupStack.Count > 0)
            {
                await Mopups.Services.MopupService.Instance.PopAsync();
            }
        }
    }
}
