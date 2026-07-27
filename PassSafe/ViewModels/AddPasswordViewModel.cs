namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using CommunityToolkit.Mvvm.Messaging;
    using MauiIcons.Core;
    using MauiIcons.Material.Sharp;
    using Microsoft.Maui.Graphics;
    using Microsoft.Maui.Storage;
    using PassSafe.Helpers;
    using PassSafe.Messages;
    using PassSafe.Models;
    using PassSafe.Services;
    using PassSafe.Views;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Manages the popup for adding a new password or editing an existing one in the Vault.
    /// Handles icons, categories, and password strength analysis.
    /// </summary>
    public partial class AddPasswordViewModel : ObservableObject
    {
        public LocalizationManager Loc => LocalizationManager.Instance;

        internal IDatabaseService _databaseService;
        internal ICryptoService _cryptoService;
        internal IDialogService _dialogService;
        internal SafeViewModel _sfvm;

        [ObservableProperty] private int passwordId = 0;
        [ObservableProperty] private string popupTitle;
        [ObservableProperty] private string actionButtonText;
        [ObservableProperty] private string title = string.Empty;
        [ObservableProperty] private string userName = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SecurityProgress))]
        [NotifyPropertyChangedFor(nameof(SecurityStatus))]
        [NotifyPropertyChangedFor(nameof(SecurityColor))]
        private string password = string.Empty;

        [ObservableProperty] private ObservableCollection<string> categories;
        [ObservableProperty] private string selectedCategory;
        [ObservableProperty] private string newCategoryName;

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

        /// <summary>
        /// Initializes services and subscribes to popup close events to clear memory.
        /// </summary>
        public AddPasswordViewModel(IDatabaseService databaseService, ICryptoService cryptoService, IDialogService dialogService, SafeViewModel sfvm)
        {
            _databaseService = databaseService;
            _cryptoService = cryptoService;
            _dialogService = dialogService;
            _sfvm = sfvm;

            PopupTitle = Loc["TitleAddPass"];
            ActionButtonText = Loc["AddBtn"];

            LoadCategories();

            // Clear inputs when popup is closed to avoid data leaks
            Mopups.Services.MopupService.Instance.Popped += (s, e) =>
            {
                PasswordId = 0;
                Password = string.Empty;
                UserName = string.Empty;
                Title = string.Empty;
                CurrentIconIndex = 0;
                PopupTitle = Loc["TitleAddPass"];
                ActionButtonText = Loc["AddBtn"];
                SelectedCategory = Categories.FirstOrDefault();

                NextCommand.NotifyCanExecuteChanged();
                PreviousCommand.NotifyCanExecuteChanged();
            };
        }

        /// <summary>
        /// Loads user-defined custom categories from the device preferences.
        /// </summary>
        private void LoadCategories()
        {
            var cats = new List<string>();

            var customCats = Preferences.Get("CustomCategories", "");
            if (!string.IsNullOrEmpty(customCats))
            {
                cats.AddRange(customCats.Split(','));
            }

            Categories = new ObservableCollection<string>(cats);
            SelectedCategory = Categories.FirstOrDefault();
        }

        /// <summary>
        /// Creates a new custom category, saves it locally, and broadcasts it to the main page.
        /// </summary>
        [RelayCommand]
        private void AddNewCategory()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName)) return;

            string cat = NewCategoryName.Trim();

            if (!Categories.Contains(cat))
            {
                Categories.Add(cat);

                var custom = Preferences.Get("CustomCategories", "");
                custom = string.IsNullOrEmpty(custom) ? cat : custom + "," + cat;
                Preferences.Set("CustomCategories", custom);

                // Notify SafeViewModel to add the new category to the horizontal menu
                WeakReferenceMessenger.Default.Send(new CategoryAddedMessage(cat));
            }

            SelectedCategory = cat;
            NewCategoryName = string.Empty;
        }

        // --- ICON NAVIGATION METHODS ---

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

        // --- PASSWORD SECURITY ANALYSIS ---

        public double SecurityProgress => (double)CalculatePasswordScore() / 5;

        public string SecurityStatus => CalculatePasswordScore() switch
        {
            0 => Loc["SecEmpty"],
            1 => Loc["SecVeryWeak"],
            2 => Loc["SecWeak"],
            3 => Loc["SecMedium"],
            4 => Loc["SecStrong"],
            5 => Loc["SecPerfect"],
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

        /// <summary>
        /// Populates the popup fields with existing data when editing a password.
        /// </summary>
        public void LoadPasswordForEdit(Password pwd, string decryptedPass)
        {
            PasswordId = pwd.Id;
            Title = pwd.Title;
            UserName = pwd.UserName;
            Password = decryptedPass;

            if (!string.IsNullOrEmpty(pwd.Category) && Categories.Contains(pwd.Category))
                SelectedCategory = pwd.Category;

            var index = _icons.FindIndex(x => x.Icon.ToString() == pwd.Icon);
            CurrentIconIndex = index >= 0 ? index : 0;

            PopupTitle = Loc["TitleEditPass"];
            ActionButtonText = Loc["UpdateBtn"];
        }

        /// <summary>
        /// Evaluates the password strength based on length, cases, numbers, and symbols. Max score is 5.
        /// </summary>
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

        /// <summary>
        /// Encrypts the raw password and saves (or updates) the record in the SQLite database.
        /// </summary>
        [RelayCommand]
        private async Task AddPasswordAsync()
        {
            var masterPass = await SecureStorage.GetAsync("masterPass");

            if (string.IsNullOrEmpty(masterPass))
            {
                var result = await _dialogService.ShowConfirmAsync(Loc["ErrorTitle"], Loc["NoMasterPassFound"], Loc["SetBtn"], Loc["CancelBtn"]);
                if (result == true)
                {
                    await _dialogService.ShowPopupAsync(new SetMasterPassPopup());
                    return;
                }
            }

            var encrypted = _cryptoService.Encrypt(Password, masterPass);

            var data = new Password()
            {
                Id = PasswordId > 0 ? PasswordId : 0,
                Title = Title,
                UserName = UserName,
                Icon = CurrentIcon.Icon.ToString(),
                Category = SelectedCategory,
                SecurityProgress = SecurityProgress,
                SecurityStatus = SecurityStatus,
                EncryptedPassword = encrypted
            };

            if (PasswordId > 0)
                await _databaseService.UpdatePasswordAsync(data);
            else
                await _databaseService.AddPasswordAsync(data);

            // Refresh vault list
            await _sfvm.LoadPasswordsCommand.ExecuteAsync(null);

            if (Mopups.Services.MopupService.Instance.PopupStack.Count > 0)
                await Mopups.Services.MopupService.Instance.PopAsync();
        }
    }
}