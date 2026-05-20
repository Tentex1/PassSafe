using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using PassSafe.Messages;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PassSafe.ViewModels
{
    public partial class AddPasswordViewModel : ObservableObject
    {
        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string userName;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SecurityProgress))]
        [NotifyPropertyChangedFor(nameof(SecurityStatus))]
        [NotifyPropertyChangedFor(nameof(SecurityColor))]
        private string password;

        public double SecurityProgress => (double)CalculatePasswordScore() / 5;

        public string SecurityStatus => CalculatePasswordScore() switch
        {
            0 => "Empty",
            1 => "Very Weak ❌",
            2 => "Weak ⚠️",
            3 => "Medium 🧐",
            4 => "Strong 💪",
            5 => "Legendary Secure 🔥",
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

        public AddPasswordViewModel()
        {
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
            var data = new PasswordTransferData(Title, UserName, Password);

            WeakReferenceMessenger.Default.Send(new PasswordAddedMessage(data));

            await Mopups.Services.MopupService.Instance.PopAsync();
        }
    }
}