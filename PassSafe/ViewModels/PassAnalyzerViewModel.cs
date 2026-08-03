namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using Microsoft.Maui.Storage;
    using PassSafe.Helpers;
    using PassSafe.Models;
    using PassSafe.Services;
    using PassSafe.Views;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Analyzes all saved passwords in the vault to calculate a security score and identify vulnerabilities.
    /// </summary>
    public partial class PassAnalyzerViewModel : ObservableObject
    {
        public LocalizationManager Loc => LocalizationManager.Instance;

        private readonly IDatabaseService _databaseService;
        private readonly ICryptoService _cryptoService;
        private readonly IDialogService _dialogService;

        private string masterPass;

        [ObservableProperty]
        private int securityScore;

        [ObservableProperty]
        private string generalStatusText;

        [ObservableProperty]
        private string generalStatusDescription;

        [ObservableProperty]
        private bool isRefreshing;

        public ObservableCollection<AnalysisCard> AnalysisCards { get; set; } = new();
        public ObservableCollection<CriticalAction> CriticalActions { get; set; } = new();

        public PassAnalyzerViewModel(IDatabaseService databaseService, ICryptoService cryptoService, IDialogService dialogService)
        {
            _databaseService = databaseService;
            _cryptoService = cryptoService;
            _dialogService = dialogService;

            IsRefreshing = true;
        }

        [RelayCommand]
        private async Task ChangePasswordAsync(Password password)
        {
            var vm = App.Services.GetService<AddPasswordViewModel>();
            string decrypted = _cryptoService.Decrypt(password.EncryptedPassword, masterPass);
            vm.LoadPasswordForEdit(password, decrypted);
            await Mopups.Services.MopupService.Instance.PushAsync(new AddPasswordPopup(vm));
        }

        /// <summary>
        /// Runs a heavy background task to decrypt and check all passwords.
        /// Prevents UI freezing by using Task.Run.
        /// </summary>
        [RelayCommand]
        private async Task RunAnalysisAsync()
        {
            try
            {
                IsRefreshing = true;

                if (string.IsNullOrEmpty(masterPass))
                    masterPass = await SecureStorage.GetAsync("masterPass");

                var encryptedData = await _databaseService.GetDatabaseAsync();

                if (encryptedData == null || !encryptedData.Any())
                {
                    ResetToEmptyState();
                    return;
                }

                var analysisResult = await Task.Run(() =>
                {
                    int strong = 0, weak = 0, risky = 0;
                    var tempCriticals = new List<CriticalAction>();

                    var decryptedList = encryptedData.Select(pwd => new
                    {
                        Original = pwd,
                        PlainText = _cryptoService.Decrypt(pwd.EncryptedPassword, masterPass)
                    }).ToList();

                    var passwordCounts = decryptedList.GroupBy(x => x.PlainText).ToDictionary(g => g.Key, g => g.Count());

                    foreach (var item in decryptedList)
                    {
                        bool isWeak = false;
                        bool isRisky = false;

                        bool isReused = passwordCounts.TryGetValue(item.PlainText, out int count) && count > 1;

                        bool isStrongTier = IsStrongPassword(item.PlainText);

                        if (!isStrongTier)
                        {
                            weak++;
                            isWeak = true;

                            tempCriticals.Add(new CriticalAction
                            {
                                Title = item.Original.Title,
                                Description = $"{Loc["AnaWeakDesc"]}: \"{item.PlainText}\"",
                                IconKey = item.Original.Icon,
                                Color = "#FF5252",
                                TargetPassword = item.Original
                            });
                        }

                        if (isReused)
                        {
                            risky++;
                            isRisky = true;

                            if (!tempCriticals.Any(a => a.TargetPassword == item.Original && a.Description.Contains(Loc["ReusedPassDesc"])))
                            {
                                tempCriticals.Add(new CriticalAction
                                {
                                    Title = item.Original.Title,
                                    Description = Loc["ReusedPassDesc"],
                                    IconKey = item.Original.Icon,
                                    Color = "#FFAB40",
                                    TargetPassword = item.Original
                                });
                            }
                        }

                        if (!isWeak && !isRisky)
                        {
                            strong++;
                        }
                    }

                    return new { Strong = strong, Weak = weak, Risky = risky, Criticals = tempCriticals };
                });

                // UI Güncellemeleri
                CriticalActions.Clear();
                foreach (var action in analysisResult.Criticals)
                {
                    CriticalActions.Add(action);
                }

                AnalysisCards.Clear();
                AnalysisCards.Add(new AnalysisCard { Title = Loc["AnaStrong"], Count = analysisResult.Strong, SideColor = "#20E19B", Description = Loc["AnaStrongDesc"], IconKey = "VerifiedUser" });
                AnalysisCards.Add(new AnalysisCard { Title = Loc["AnaWeak"], Count = analysisResult.Weak, SideColor = "#FF5252", Description = Loc["AnaWeakDesc"], IconKey = "Warning" });
                AnalysisCards.Add(new AnalysisCard { Title = Loc["AnaRisky"], Count = analysisResult.Risky, SideColor = "#FFAB40", Description = Loc["AnaRiskyDesc"], IconKey = "ContentCopy" });

                CalculateScore(analysisResult.Strong, encryptedData.Count);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Analyzer Error] -> {ex.Message}");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        /// <summary>
        /// Şifrenin "Strong Tier" standartlarına uyup uymadığını kontrol eder.
        /// Strong kriterinin altında kalan her şey (Medium, Weak, Very Weak) false döner.
        /// </summary>
        private static bool IsStrongPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;

            // Minimum 10-12 karakter uzunluğu
            if (password.Length < 10) return false;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            // Strong olması için: En az 10 karakter + Büyük Harf + Küçük Harf + Rakam + Özel Karakter
            return hasUpper && hasLower && hasDigit && hasSpecial;
        }

        /// <summary>
        /// Calculates the final security score percentage and updates the UI status texts.
        /// </summary>
        private void CalculateScore(int strongCount, int totalCount)
        {
            if (totalCount > 0)
                SecurityScore = (int)((double)strongCount / totalCount * 100);
            else
                SecurityScore = 100;

            if (SecurityScore >= 80)
            {
                GeneralStatusText = Loc["AnaStatusPerfect"];
                GeneralStatusDescription = Loc["AnaStatusPerfectDesc"];
            }
            else if (SecurityScore >= 50)
            {
                GeneralStatusText = Loc["AnaStatusGood"];
                GeneralStatusDescription = Loc["AnaStatusGoodDesc"];
            }
            else
            {
                GeneralStatusText = Loc["AnaStatusRisk"];
                GeneralStatusDescription = Loc["AnaStatusRiskDesc"];
            }
        }

        private void ResetToEmptyState()
        {
            SecurityScore = 100;
            GeneralStatusText = Loc["AnaEmpty"];
            GeneralStatusDescription = Loc["AnaEmptyDesc"];
            CriticalActions.Clear();
            AnalysisCards.Clear();
            IsRefreshing = false;
        }

        partial void OnIsRefreshingChanged(bool value)
        {
            if (value)
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await RunAnalysisAsync();
                });
            }
        }
    }
}