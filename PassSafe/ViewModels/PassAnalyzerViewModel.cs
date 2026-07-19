using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Storage;
using PassSafe.Models;
using PassSafe.Services;
using PassSafe.Views;

namespace PassSafe.ViewModels
{
    public partial class PassAnalyzerViewModel : ObservableObject
    {
        private readonly IDatabaseService _databaseService;
        private readonly ICryptoService _cryptoService;
        private readonly IDialogService _dialogService;

        private string master_pass;

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

            string decrypted = _cryptoService.Decrypt(password.EncryptedPassword, master_pass);

            vm.LoadPasswordForEdit(password, decrypted);

            await Mopups.Services.MopupService.Instance.PushAsync(new AddPasswordPopup(vm));
        }

        [RelayCommand]
        private async Task RunAnalysisAsync()
        {
            try
            {
                master_pass = await SecureStorage.GetAsync("master_pass");
                var encryptedData = await _databaseService.GetDatabaseAsync();

                if (encryptedData == null || !encryptedData.Any())
                {
                    ResetToEmptyState();
                    return;
                }

                CriticalActions.Clear();
                AnalysisCards.Clear();

                int strongCount = 0;
                int weakCount = 0;
                int riskyCount = 0;

                var decryptedList = new List<(Password Original, string PlainText)>();
                foreach (var pwd in encryptedData)
                {
                    string plainText = _cryptoService.Decrypt(pwd.EncryptedPassword, master_pass);
                    decryptedList.Add((pwd, plainText));
                }

                var passwordCounts = decryptedList.GroupBy(x => x.PlainText).ToDictionary(g => g.Key, g => g.Count());

                foreach (var item in decryptedList)
                {
                    bool isWeak = false;
                    bool isRisky = false;

                    if (item.PlainText.Length < 8 || item.PlainText == "123456" || item.PlainText == "12345678")
                    {
                        weakCount++;
                        isWeak = true;

                        CriticalActions.Add(new CriticalAction
                        {
                            Title = item.Original.Title,
                            Description = $"Çok zayıf şifre: \"{item.PlainText}\"",
                            IconKey = item.Original.Icon,
                            Color = "#FF5252",
                            TargetPassword = item.Original
                        });
                    }

                    if (passwordCounts.TryGetValue(item.PlainText, out int count) && count > 1)
                    {
                        riskyCount++;
                        isRisky = true;

                        if (!CriticalActions.Any(a => a.Title == item.Original.Title && a.Description.Contains("Tekrar eden")))
                        {
                            CriticalActions.Add(new CriticalAction
                            {
                                Title = item.Original.Title,
                                Description = "Tekrar eden şifre kullanılıyor",
                                IconKey = item.Original.Icon,
                                Color = "#FFAB40",
                                TargetPassword = item.Original
                            });
                        }
                    }

                    if (!isWeak && !isRisky)
                    {
                        strongCount++;
                    }
                }

                AnalysisCards.Add(new AnalysisCard { Title = "Güçlü", Count = strongCount, SideColor = "#20E19B", Description = "Karmaşık ve uzun karakterli güvenli şifreler.", IconKey = "VerifiedUser" });
                AnalysisCards.Add(new AnalysisCard { Title = "Zayıf", Count = weakCount, SideColor = "#FF5252", Description = "8 karakterden kısa veya çok basit dizimler.", IconKey = "Warning" });
                AnalysisCards.Add(new AnalysisCard { Title = "Riskli", Count = riskyCount, SideColor = "#FFAB40", Description = "Birden fazla hesapta tekrar eden şifreler.", IconKey = "ContentCopy" });

                CalculateScore(strongCount, encryptedData.Count);
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

        private void CalculateScore(int strongCount, int totalCount)
        {
            if (totalCount > 0)
            {
                SecurityScore = (int)((double)strongCount / totalCount * 100);
            }
            else
            {
                SecurityScore = 100;
            }

            if (SecurityScore >= 80)
            {
                GeneralStatusText = "Mükemmel";
                GeneralStatusDescription = "Dijital kasanız tamamen güvende görünüyor.";
            }
            else if (SecurityScore >= 50)
            {
                GeneralStatusText = "İyi";
                GeneralStatusDescription = "Dijital kasanız güvende görünüyor, ancak birkaç iyileştirme ile kusursuz olabilir.";
            }
            else
            {
                GeneralStatusText = "Risk Altında";
                GeneralStatusDescription = "Kasanızda kritik güvenlik açıkları var. Lütfen zayıf şifreleri güncelleyin!";
            }
        }

        private void ResetToEmptyState()
        {
            SecurityScore = 100;
            GeneralStatusText = "Kasa Boş";
            GeneralStatusDescription = "Kasanızda henüz kayıtlı bir parola bulunmuyor.";
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