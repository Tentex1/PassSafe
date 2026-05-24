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
        internal string master_pass;

        public IDialogService _dialogService;

        public IDatabaseService _databaseService;

        public ICryptoService _cryptoService;

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

            WeakReferenceMessenger.Default.Register<PasswordAddedMessage>(this, async (r, m) =>
            {
                var data = m.Value;

                string encrypted = _cryptoService.Encrypt(data.Password, master_pass);

                Password newPass = new()
                {
                    Title = data.Title,
                    UserName = data.UserName,
                    Icon = data.Icon,
                    EncryptedPassword = encrypted,
                    SecurityStatus = data.SecurityStatus,
                    SecurityProgress = data.SecurityProgress
                };

                await _databaseService.AddPassword(newPass);
                await LoadPasswords();        

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await LoadPasswords();
                    await _dialogService.ShowAlertAsync("Başarılı", "Yeni şifre kasaya güvenle eklendi!", "Tamam");
                });
            });

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        private async Task LoadPasswords()
        {
            Passwords = null;
            Passwords = new(await _databaseService.GetDatabase());
        }

        private async Task CheckMasterPass()
        {
            try
            {
                master_pass = await SecureStorage.GetAsync("master_pass");

                if (master_pass != null)
                {
                    await _databaseService.InitializeDatabase(master_pass);

                    var dbDatas = await _databaseService.GetDatabase();
                    if (dbDatas != null)
                    {
                        Passwords = new(dbDatas);
                    }
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
                        Application.Current?.Quit();
                    }
                }
            }
            catch (Exception ex) { await _dialogService.ShowAlertAsync("Error", ex.Message, "Copy"); }
        }

        public void Receive(AuthResultMessage message)
        {
            var response = message.Value;

            if (response.Status == BiometricResponseStatus.Success)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await CheckMasterPass();
                });
            }
        }

        partial void OnIsRefreshingChanged(bool value)
        {
            if (value == true)
            {
                _ = LoadPasswords();
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private async Task ShowAddPasswordPopup()
        {
            await _dialogService.ShowPopup(new AddPasswordPopup());
        }

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
            }
            IsRefreshing = true;
        }
    }
}
