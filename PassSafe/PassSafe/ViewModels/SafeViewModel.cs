namespace PassSafe.ViewModels
{
    using CommunityToolkit.Maui.Alerts;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging;
    using Microsoft.Maui.ApplicationModel;
    using PassSafe.Messages;
    using PassSafe.Models;
    using PassSafe.Services;
    using PassSafe.Views;
    using Plugin.Maui.Biometric;
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    public partial class SafeViewModel : ObservableObject, IRecipient<AuthResultMessage>
    {
        private string master_pass;

        public IDialogService _dialogService;
        public IDatabaseService _databaseService;
        public ICryptoService _cryptoService;

        [ObservableProperty]
        private string dbStatus;

        [ObservableProperty]
        private ObservableCollection<Password> passwords;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isRefreshing;

        public SafeViewModel(ICryptoService cryptoService, IDialogService dialogService, IDatabaseService databaseService)
        {
            _dialogService = dialogService;
            _databaseService = databaseService;
            _cryptoService = cryptoService;

            // Mesaj dinleyicisini kayıt ediyoruz
            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        /// <summary>
        /// Veritabanından verileri asenkron ve güvenli çeken ana metot
        /// </summary>
        [RelayCommand]
        private async Task LoadPasswordsAsync()
        {
            try
            {
                var dbDatas = await _databaseService.GetDatabase();
                if (dbDatas != null)
                {
                    Passwords = new ObservableCollection<Password>(dbDatas);
                    DbStatus = Passwords.Count <= 0 ? "Veriler Yüklendi, Hiç Parolanız yok." : "Veriler Yüklendi";
                }
                else
                {
                    Passwords = new ObservableCollection<Password>();
                    DbStatus = "Veritabanı boş veya yüklenemedi.";
                }
            }
            catch (Exception ex)
            {
                DbStatus = "Veri yükleme hatası!";
                System.Diagnostics.Debug.WriteLine($"[Load Error] -> {ex.Message}");
            }
        }

        private async Task CheckMasterPassAsync()
        {
            try
            {
                master_pass = await SecureStorage.GetAsync("master_pass");

                if (master_pass != null)
                {
                    await _databaseService.InitializeDatabase(master_pass);

                    // Veritabanı başarıyla init edildiyse şifreleri yükle komutunu çağırıyoruz
                    await LoadPasswordsAsync();
                }
                else
                {
                    var result = await _dialogService.ShowConfirmAsync("Hata", "Ana şifre belirlememişsiniz", "Belirle", "İptal");
                    if (result == true)
                    {
                        await _dialogService.ShowPopup(new SetMasterPassPopup());
                    }
                    else
                    {
#if ANDROID
                        Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#else
                            Application.Current?.Quit();
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync("Error", ex.Message, "Copy");
            }
        }

        /// <summary>
        /// Biyometrik doğrulama bittiğinde tetiklenen event (Thread-Safe)
        /// </summary>
        public void Receive(AuthResultMessage message)
        {
            var response = message.Value;

            if (response.Status == BiometricResponseStatus.Success)
            {
                // UI Thread'ini kitlemeden asenkron akışı sırasıyla işletiyoruz
                Task.Run(async () =>
                {
                    await CheckMasterPassAsync();
                });
            }
        }

        partial void OnIsRefreshingChanged(bool value)
        {
            if (value)
            {
                DbStatus = "Veriler denetleniyor";
                Task.Run(async () =>
                {
                    await LoadPasswordsAsync();

                    // UI güncellenmesini ana thread'e paslıyoruz
                    MainThread.BeginInvokeOnMainThread(() => IsRefreshing = false);
                });
            }
        }

        [RelayCommand]
        private async Task ShowAddPasswordPopup() => await _dialogService.ShowPopup(new AddPasswordPopup());

        [RelayCommand]
        private async Task ShowPassword(string password)
        {
            var passSolution = _cryptoService.Decrypt(password, master_pass);
            await _dialogService.ShowAlertAsync("", passSolution, "OK");
        }

        [RelayCommand]
        private async Task CopyPassword(string password)
        {
            var pass = _cryptoService.Decrypt(password, master_pass);
            await Clipboard.SetTextAsync(pass);
            await Toast.Make("Şifre panoya kopyalandı", CommunityToolkit.Maui.Core.ToastDuration.Short, 14).Show();
        }

        [RelayCommand]
        private async Task DeletePassword(int id)
        {
            var dialog = await _dialogService.ShowConfirmAsync("Delete Password?", "Are you sure you want to delete this password? This action cannot be undone.", "Delete", "Cancel");
            if (dialog == true)
            {
                await _databaseService.DeletePassword(id);
                IsRefreshing = true; // Yenilemeyi tetikler
            }
        }
    }
}