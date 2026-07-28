namespace PassSafe.ViewModels
{
    using CommunityToolkit.Maui.Alerts;
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging;
    using Microsoft.Maui.ApplicationModel;
    using PassSafe.Helpers;
    using PassSafe.Messages;
    using PassSafe.Models;
    using PassSafe.Services;
    using PassSafe.Views;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Represents a category item in the horizontal filter menu.
    /// </summary>
    public partial class CategoryItem : ObservableObject
    {
        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private bool isSelected;
    }

    /// <summary>
    /// Manages the main Vault page. Handles displaying, searching, filtering, and deleting passwords.
    /// </summary>
    public partial class SafeViewModel : ObservableObject, IRecipient<CategoryAddedMessage>
    {
        public LocalizationManager Loc => LocalizationManager.Instance;

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
        private string selectedCategory;

        [ObservableProperty]
        private string searchQuery;

        [ObservableProperty]
        private string dbStatus;

        [ObservableProperty]
        private bool isRefreshing;

        /// <summary>
        /// Initializes the view model, loads custom categories, and handles live translation changes.
        /// </summary>
        public SafeViewModel(ICryptoService cryptoService, IDialogService dialogService, IDatabaseService databaseService)
        {
            _cryptoService = cryptoService;
            _dialogService = dialogService;
            _databaseService = databaseService;

            SelectedCategory = Loc["CatAll"];

            Categories = new ObservableCollection<CategoryItem>
            {
                new CategoryItem { Name = Loc["CatAll"], IsSelected = true },
                new CategoryItem { Name = Loc["CatFavorites"], IsSelected = false }
            };

            // Load user's custom categories from device memory
            var customCats = Preferences.Get("CustomCategories", "");
            if (!string.IsNullOrEmpty(customCats))
            {
                foreach (var cat in customCats.Split(','))
                {
                    Categories.Add(new CategoryItem { Name = cat, IsSelected = false });
                }
            }

            // Live language change listener to instantly translate default categories
            LocalizationManager.Instance.PropertyChanged += (s, e) =>
            {
                if (Categories != null && Categories.Count >= 2)
                {
                    bool wasAllSelected = Categories[0].IsSelected;
                    bool wasFavSelected = Categories[1].IsSelected;

                    Categories[0].Name = Loc["CatAll"];
                    Categories[1].Name = Loc["CatFavorites"];

                    if (wasAllSelected) SelectedCategory = Loc["CatAll"];
                    else if (wasFavSelected) SelectedCategory = Loc["CatFavorites"];

                    FilterPasswords();
                }
            };

            WeakReferenceMessenger.Default.RegisterAll(this);
            _ = LoadPasswordsAsync();
        }

        /// <summary>
        /// Listens for new categories created in the AddPasswordPopup.
        /// </summary>
        public void Receive(CategoryAddedMessage message)
        {
            if (!Categories.Any(c => c.Name == message.Value))
            {
                Categories.Add(new CategoryItem { Name = message.Value, IsSelected = false });
            }
        }

        /// <summary>
        /// Triggers when the user types in the search bar. Instantly filters the list.
        /// </summary>
        partial void OnSearchQueryChanged(string value)
        {
            FilterPasswords();
        }

        /// <summary>
        /// Loads all passwords from the database into the memory securely.
        /// </summary>
        [RelayCommand]
        private async Task LoadPasswordsAsync()
        {
            IsRefreshing = true;
            DbStatus = Loc["DbLoading"];
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
            catch (Exception)
            {
                DbStatus = Loc["ErrorDataLoad"];
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        /// <summary>
        /// Highlights the selected category button and filters the list.
        /// </summary>
        [RelayCommand]
        private void SelectCategory(CategoryItem category)
        {
            if (SelectedCategory == category.Name) return;

            foreach (var cat in Categories) cat.IsSelected = false;

            category.IsSelected = true;
            SelectedCategory = category.Name;
            FilterPasswords();
        }

        /// <summary>
        /// Filters the passwords using LINQ based on the selected category and search query.
        /// </summary>
        private void FilterPasswords()
        {
            if (_allPasswords == null) return;
            IEnumerable<Password> filteredList;

            // 1. Filter by Category
            if (SelectedCategory == Loc["CatAll"])
                filteredList = _allPasswords;
            else if (SelectedCategory == Loc["CatFavorites"])
                filteredList = _allPasswords.Where(p => p.IsFavorited);
            else
                filteredList = _allPasswords.Where(p => p.Category == SelectedCategory);

            // 2. Filter by Search Query
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var query = SearchQuery.ToLowerInvariant();
                filteredList = filteredList.Where(p =>
                    (p.Title != null && p.Title.ToLowerInvariant().Contains(query)) ||
                    (p.UserName != null && p.UserName.ToLowerInvariant().Contains(query)));
            }

            CollectionViewItemSource = new ObservableCollection<Password>(filteredList);

            // 3. Update Empty State Message
            if (!CollectionViewItemSource.Any())
            {
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                    DbStatus = Loc["SearchNoResults"];
                else
                    DbStatus = SelectedCategory == Loc["CatAll"] ? Loc["DbEmpty"] : Loc["DbNoCategory"];
            }
            else
            {
                DbStatus = string.Empty;
            }
        }

        [RelayCommand]
        private async Task ShowAddPasswordPopup()
        {
            var vm = App.Services.GetService<AddPasswordViewModel>();
            await _dialogService.ShowPopupAsync(new AddPasswordPopup(vm));
        }

        /// <summary>
        /// Opens the popup with the decrypted password details for editing.
        /// </summary>
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

        /// <summary>
        /// Toggles the password visibility inline. Decrypts and shows or hides it.
        /// </summary>
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
            await Toast.Make(Loc["MsgCopied"]).Show();
        }

        [RelayCommand]
        private async Task DeletePasswordAsync(Password password)
        {
            var dialog = await _dialogService.ShowConfirmAsync(Loc["ConfirmDeleteTitle"], Loc["ConfirmDeleteDesc"], Loc["DeleteBtn"], Loc["CancelBtn"]);
            if (dialog == true)
            {
                await _databaseService.DeletePasswordAsync(password.Id);
                _allPasswords.Remove(password);
                FilterPasswords(); // Update UI without hitting database again
            }
        }

        [RelayCommand]
        private async Task SetFavoritePasswordAsync(Password password)
        {
            password.IsFavorited = !password.IsFavorited;
            await _databaseService.UpdatePasswordAsync(password);
            FilterPasswords();
            IsRefreshing = true;
            await Toast.Make(password.IsFavorited ? $"{password.Title} {Loc["MsgFavAdded"]}" : $"{password.Title} {Loc["MsgFavRemoved"]}").Show();
        }
    }
}