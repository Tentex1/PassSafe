namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging; // Telsiz kütüphanesini ekledik
    using Microsoft.Maui.ApplicationModel;
    using PassSafe.Messages; // Yeni oluşturduğumuz mesajları ekledik
    using PassSafe.Models;
    using PassSafe.Services;
    using PassSafe.Views;
    using Plugin.Maui.Biometric;
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines the <see cref="VaultViewModel" />
    /// </summary>
    public partial class VaultViewModel : ObservableObject, IRecipient<AuthResultMessage>
    {
        internal string master_pass;

        public IDialogService _dialogService;

        public IDatabaseService _databaseService;

        public ICryptoService _cryptoService;

        [ObservableProperty]
        private ObservableCollection<Password> passwords;

        [ObservableProperty]
        private string password;

        public VaultViewModel(ICryptoService cryptoService, IDialogService dialogService, IDatabaseService databaseService)
        {
            _dialogService = dialogService;
            _databaseService = databaseService;
            _cryptoService = cryptoService;

            // --- TELSİZİ BURADA AÇIYORUZ ---
            WeakReferenceMessenger.Default.Register<PasswordAddedMessage>(this, (r, m) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    var gelenPaket = m.Value;

                    Password yeniSifre = new()
                    {
                        Title = gelenPaket.UserName,
                        UserName = "kullanici@mail.com",
                        EncryptedPassword = _cryptoService.Encrypt(gelenPaket.Password, master_pass)
                    };

                    await _databaseService.AddPassword(yeniSifre);
                    Passwords = new(await _databaseService.GetDatabase());
                    await _dialogService.ShowAlertAsync("Başarılı", "Yeni şifre kasaya güvenle eklendi!", "Tamam");
                });
            });
            // --------------------------------

            // =========================================================================
            // 2. KRİTİK DEĞİŞİKLİK: RegisterAll(this) sayesinde yukarıya yazdığımız IRecipient otomatik aktif olur.
            // =========================================================================
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        private async Task CheckMasterPass()
        {
            try
            {
                master_pass = await SecureStorage.GetAsync("master_pass");

                if (master_pass != null)
                {
                    await _databaseService.InitializeDatabase(master_pass);

                    // Veritabanı başarıyla açıldıktan sonra şifreleri ekrana dolduruyoruz
                    var dbGelen = await _databaseService.GetDatabase();
                    if (dbGelen != null)
                    {
                        Passwords = new(dbGelen);
                    }
                }
                else
                {
                    var result = await _dialogService.ShowConfirmAsync("Hata", "Ana şifre belirlememişsiniz", "Belirle", "İptal");
                    if (result == true)
                    {
                        _dialogService.ShowPopup(new SetMasterPassPopup());
                    }
                    else
                    {
                        Application.Current?.Quit();
                    }
                }
            }
            catch (Exception ex) { await _dialogService.ShowAlertAsync("Error", ex.Message, "Copy"); }
        }

        // =========================================================================
        // 3. KRİTİK DEĞİŞİKLİK: Receive metodu async Task olamaz, void olmak zorunda.
        // İçindeki async işleri (CheckMasterPass) MainThread veya Task.Run ile tetikliyoruz.
        // NOT: ShellViewModel'den gelen nesne kütüphanene göre BiometricResponse veya AuthenticationResponse olabilir, hangisiyse onu yazarsın.
        // =========================================================================
        public void Receive(AuthResultMessage message)
        {
            var response = message.Value;

            if (response.Status == BiometricResponseStatus.Success)
            {
                // Async metodu void içinden güvenli şekilde tetikliyoruz
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await CheckMasterPass();
                });
            }
        }

        [RelayCommand]
        private void ShowAddPasswordPopup()
        {
            _dialogService.ShowPopup(new AddPasswordPopup());
        }
    }
}
