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

        // =========================================================================
        // YENİ EKLEDİĞİMİZ KISIM: < O > MANTIĞI (SİMGE SEÇİCİ)
        // =========================================================================

        // 1. Kullanabileceğimiz simgelerin listesi (Burayı istediğin gibi doldur)
        private readonly List<string> _simgeler = new List<string> { "🔑", "🛡️", "👤", "💼", "🌐", "📧" };

        // 2. Şu an kaçıncı simgedeyiz? (0'dan başlar, yani ilk simge "🔑")
        // Her değiştiğinde ekrandaki "SuAnkiSimge" alanını da otomatik yenilet diyoruz.
        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SuAnkiSimge))]
        private int _suAnkiIndeks = 0;

        // 3. Ekranda "O" harfi yerine görünecek olan güncel simge (Dışarıya açılan kapı)
        public string SuAnkiSimge => _simgeler[SuAnkiIndeks];

        // 4. SONRAKİ BUTONU (>) İÇİN KOMUT
        // CanSonraki metodu "true" döndüğü sürece bu buton tıklanabilir olur.
        [RelayCommand(CanExecute = nameof(CanSonraki))]
        private void Sonraki()
        {
            SuAnkiIndeks++; // Bir sonraki simgeye geç

            // Butonların aktiflik durumunu (kilitlenip kilitlenmeyeceğini) tetikliyoruz
            SonrakiCommand.NotifyCanExecuteChanged();
            OncekiCommand.NotifyCanExecuteChanged();
        }
        // Listenin sonuna gelmediysek "true" döner, sonundaysak "false" döner (Buton kilitlenir)
        private bool CanSonraki() => SuAnkiIndeks < _simgeler.Count - 1;

        // 5. ÖNCEKİ BUTONU (<) İÇİN KOMUT
        [RelayCommand(CanExecute = nameof(CanOnceki))]
        private void Onceki()
        {
            SuAnkiIndeks--; // Bir önceki simgeye geç

            SonrakiCommand.NotifyCanExecuteChanged();
            OncekiCommand.NotifyCanExecuteChanged();
        }
        // Eğer 0. indeksten büyüksek geriye gidebiliriz demektir
        private bool CanOnceki() => SuAnkiIndeks > 0;

        // =========================================================================

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

        // MAUI'de şifre kaydederken seçilen simgeyi de göndermek istersen, 
        // SuAnkiSimge property'sini buradaki transfer verisine ekleyebilirsin.
        [RelayCommand]
        private async Task AddPassword()
        {
            var data = new PasswordTransferData(Title, UserName, Password);

            WeakReferenceMessenger.Default.Send(new PasswordAddedMessage(data));

            await Mopups.Services.MopupService.Instance.PopAsync();
        }
    }
}