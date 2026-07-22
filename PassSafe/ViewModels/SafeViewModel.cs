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
    using PassSafe.Messages;    
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class CategoryItem : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private bool isSelected;
    }

    public partial class SafeViewModel : ObservableObject, IRecipient<CategoryAddedMessage>
    {
        private readonly IDialogService _dialogService;
        private readonly IDatabaseService _databaseService;
        private readonly ICryptoService _cryptoService;

        private string masterPass;
        private List<Password> _allPasswords = new();

        [ObservableProperty]
        private ObservableCollection<Password> collectionViewItemSource;

        [ObservableProperty]
        private ObservableCollection<CategoryItem> categories;

        [ObservableProperty]
        private string selectedCategory = "Hepsi";

        [ObservableProperty]
        private string dbStatus;

        [ObservableProperty]
        private bool isRefreshing;

        public SafeViewModel(ICryptoService cryptoService, IDialogService dialogService, IDatabaseService databaseService)
        {
            _cryptoService = cryptoService;
            _dialogService = dialogService;
            _databaseService = databaseService;

            Categories = new ObservableCollection<CategoryItem>
            {
                new CategoryItem { Name = "Hepsi", IsSelected = true },
                new CategoryItem { Name = "Favoriler", IsSelected = false },
                new CategoryItem { Name = "Sosyal Medya", IsSelected = false },
                new CategoryItem { Name = "Banka", IsSelected = false },
                new CategoryItem { Name = "İş", IsSelected = false },
                new CategoryItem { Name = "Oyun", IsSelected = false }
            };

            var customCats = Preferences.Get("CustomCategories", "");
            if (!string.IsNullOrEmpty(customCats))
            {
                foreach (var cat in customCats.Split(','))
                {
                    Categories.Add(new CategoryItem { Name = cat, IsSelected = false });
                }
            }

            WeakReferenceMessenger.Default.RegisterAll(this);
            _ = LoadPasswordsAsync();
        }

        public void Receive(CategoryAddedMessage message)
        {
            if (!Categories.Any(c => c.Name == message.Value))
            {
                Categories.Add(new CategoryItem { Name = message.Value, IsSelected = false });
            }
        }

        [RelayCommand]
        private async Task LoadPasswordsAsync()
        {
            IsRefreshing = true;
            DbStatus = "Veriler yükleniyor...";
            try
            {
                if (string.IsNullOrEmpty(masterPass))
                    masterPass = await SecureStorage.GetAsync("masterPass");

                var dbDatas = await _databaseService.GetDatabaseAsync();

                if (dbDatas != null && dbDatas.Any())
                {
                    _allPasswords = dbDatas.ToList();
                    DbStatus = string.Empty;
                }
                else
                {
                    _allPasswords = new List<Password>();
                }
                FilterPasswords();
            }
            catch (Exception ex)
            {
                DbStatus = "Veri yükleme hatası!";
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        [RelayCommand]
        private void SelectCategory(CategoryItem category)
        {
            if (SelectedCategory == category.Name) return;

            foreach (var cat in Categories) cat.IsSelected = false;

            category.IsSelected = true;
            SelectedCategory = category.Name;
            FilterPasswords();
        }

        private void FilterPasswords()
        {
            if (_allPasswords == null) return;
            IEnumerable<Password> filteredList;

            if (SelectedCategory == "Hepsi") filteredList = _allPasswords;
            else if (SelectedCategory == "Favoriler") filteredList = _allPasswords.Where(p => p.IsFavorited);
            else filteredList = _allPasswords.Where(p => p.Category == SelectedCategory);

            CollectionViewItemSource = new ObservableCollection<Password>(filteredList);

            if (!CollectionViewItemSource.Any())
                DbStatus = SelectedCategory == "Hepsi" ? "Hiç parolanız yok." : "Bu kategoride parola bulunamadı.";
            else
                DbStatus = string.Empty;
        }

        [RelayCommand]
        private async Task ShowAddPasswordPopup()
        {
            var vm = App.Services.GetService<AddPasswordViewModel>();
            await _dialogService.ShowPopupAsync(new AddPasswordPopup(vm));
        }

        [RelayCommand]
        private async Task EditPassword(Password password)
        {
            if (string.IsNullOrEmpty(masterPass))
                masterPass = await SecureStorage.GetAsync("masterPass");

            var decrypted = _cryptoService.Decrypt(password.EncryptedPassword, masterPass);

            var vm = App.Services.GetService<AddPasswordViewModel>();
            vm.LoadPasswordForEdit(password, decrypted);

            await _dialogService.ShowPopupAsync(new AddPasswordPopup(vm));
        }

        [RelayCommand]
        private async Task ShowPassword(Password password)
        {
            if (password.IsPasswordVisible)
            {
                password.DisplayPassword = "••••••••";
                password.IsPasswordVisible = false;
            }
            else
            {
                if (string.IsNullOrEmpty(masterPass)) masterPass = await SecureStorage.GetAsync("masterPass");
                password.DisplayPassword = _cryptoService.Decrypt(password.EncryptedPassword, masterPass);
                password.IsPasswordVisible = true;
            }
        }

        [RelayCommand]
        private async Task CopyPassword(string password)
        {
            if (string.IsNullOrEmpty(masterPass)) masterPass = await SecureStorage.GetAsync("masterPass");
            var pass = _cryptoService.Decrypt(password, masterPass);
            await Clipboard.SetTextAsync(pass);
            await Toast.Make("Şifre panoya kopyalandı").Show();
        }

        [RelayCommand]
        private async Task DeletePasswordAsync(Password password)
        {
            var dialog = await _dialogService.ShowConfirmAsync("Parolayı Sil", "Bu parolayı silmek istediğinize emin misiniz?", "Sil", "İptal");
            if (dialog == true)
            {
                await _databaseService.DeletePasswordAsync(password.Id);
                _allPasswords.Remove(password);
                FilterPasswords();
            }
        }

        [RelayCommand]
        private async Task SetFavoritePasswordAsync(Password password)
        {
            password.IsFavorited = !password.IsFavorited;
            await _databaseService.UpdatePasswordAsync(password);
            FilterPasswords();
            await Toast.Make(password.IsFavorited ? "Favorilere eklendi ❤️" : "Favorilerden çıkarıldı 💔").Show();
        }
    }
}