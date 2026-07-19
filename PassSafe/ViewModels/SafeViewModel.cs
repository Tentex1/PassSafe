namespace PassSafe.ViewModels
{
    using CommunityToolkit.Maui.Alerts;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging;
    using Microsoft.Maui.ApplicationModel;
    using PassSafe.Models;
    using PassSafe.Services;
    using PassSafe.Views;
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    public partial class SafeViewModel : ObservableObject
    {
        private string master_pass;

        public IDialogService _dialogService;

        public IDatabaseService _databaseService;

        public ICryptoService _cryptoService;

        [ObservableProperty]
        private ObservableCollection<Password> collectionViewItemSource;

        [ObservableProperty]
        private string selectedCategory = "Hepsi";    

        [ObservableProperty]
        private string dbStatus;

        [ObservableProperty]
        private ObservableCollection<Password> passwords;

        [ObservableProperty]
        private ObservableCollection<Password> favoritePasswords;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isRefreshing;

        public SafeViewModel(ICryptoService cryptoService, IDialogService dialogService, IDatabaseService databaseService)
        {
            _dialogService = dialogService;
            _databaseService = databaseService;
            _cryptoService = cryptoService;

            WeakReferenceMessenger.Default.RegisterAll(this);
        }

        [RelayCommand]
        private async Task LoadPasswordsAsync()
        {
            try
            {
                master_pass = await SecureStorage.GetAsync("master_pass");
                var dbDatas = await _databaseService.GetDatabaseAsync();
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

                if (SelectedCategory == "Hepsi")
                {
                    CollectionViewItemSource = Passwords;
                }
            }
            catch (Exception ex)
            {
                DbStatus = "Veri yükleme hatası!";
                System.Diagnostics.Debug.WriteLine($"[Load Error] -> {ex.Message}");
            }
        }

        [RelayCommand]
        private async Task LoadFavoritesAsync()
        {
            try
            {
                master_pass = await SecureStorage.GetAsync("master_pass");
                var dbDatas = await _databaseService.GetFavoritesAsync();
                if (dbDatas != null)
                {
                    FavoritePasswords = new ObservableCollection<Password>(dbDatas);
                    DbStatus = Passwords.Count <= 0 ? "Veriler Yüklendi, Hiç Parolanız yok." : "Veriler Yüklendi";
                }
                else
                {
                    FavoritePasswords = new ObservableCollection<Password>();
                    DbStatus = "Veritabanı boş veya yüklenemedi.";
                }

                if (SelectedCategory == "Favoriler")
                {
                    CollectionViewItemSource = FavoritePasswords;
                }
            }
            catch (Exception ex)
            {
                DbStatus = "Veri yükleme hatası!";
                await _dialogService.ShowErrorAsync(ex);
            }
        }

        partial void OnIsRefreshingChanged(bool value)
        {
            if (value)
            {
                DbStatus = "Veriler denetleniyor";

                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await LoadPasswordsAsync();
                    await LoadFavoritesAsync();
                    IsRefreshing = false;
                });
            }
        }

        [RelayCommand]
        private async Task ShowAddPasswordPopup() => await _dialogService.ShowPopupAsync(new AddPasswordPopup());

        [RelayCommand]
        private async Task ShowPassword(string password)
        {
            if (string.IsNullOrEmpty(master_pass))
            {
                master_pass = await SecureStorage.GetAsync("master_pass");
            }
            var passSolution = _cryptoService.Decrypt(password, master_pass);
            await _dialogService.ShowAlertAsync("", passSolution, "OK");
        }

        [RelayCommand]
        private async Task CopyPassword(string password)
        {
            if (string.IsNullOrEmpty(master_pass))
            {
                master_pass = await SecureStorage.GetAsync("master_pass");
            }
            var pass = _cryptoService.Decrypt(password, master_pass);
            await Clipboard.SetTextAsync(pass);
            await Toast.Make("Şifre panoya kopyalandı", CommunityToolkit.Maui.Core.ToastDuration.Short, 14).Show();
        }

        [RelayCommand]
        private async Task DeletePasswordAsync(int id)
        {
            var dialog = await _dialogService.ShowConfirmAsync("Delete Password?", "Are you sure you want to delete this password? This action cannot be undone.", "Delete", "Cancel");
            if (dialog == true)
            {
                await _databaseService.DeletePasswordAsync(id);
                IsRefreshing = true;
            }
        }

        [RelayCommand]
        private async Task SetFavoritePasswordAsync(Password password)
        {
            password.IsFavorited = !password.IsFavorited;
            await _databaseService.UpdatePasswordAsync(password);

            await LoadFavoritesAsync();

            if (SelectedCategory == "Favoriler")
            {
                CollectionViewItemSource = FavoritePasswords;
            }

            System.Diagnostics.Debug.WriteLine("parola updatelendi");
        }

        [RelayCommand]
        private async Task SelectCategoryAsync(string categoryName)
        {
            SelectedCategory = categoryName;

            if (categoryName == "Favoriler")
            {
                await LoadFavoritesAsync();
                CollectionViewItemSource = FavoritePasswords;
            }
            else if (categoryName == "Hepsi")
            {
                await LoadPasswordsAsync();
                CollectionViewItemSource = Passwords;
            }
            else
            {
                CollectionViewItemSource = Passwords;
            }
        }
    }
}