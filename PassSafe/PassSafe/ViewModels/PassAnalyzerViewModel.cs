using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using MauiIcons.Material;
using PassSafe.Models;
using PassSafe.Services;

namespace PassSafe.ViewModels
{
    public partial class PassAnalyzerViewModel : ObservableObject
    {
        // SafeViewModel'deki gibi senin servislerin
        private readonly IDatabaseService _databaseService;
        private readonly ICryptoService _cryptoService;

        private string master_pass;

        // --- Üst Kısım Arayüz Bağlantıları ---
        [ObservableProperty]
        private int securityScore;

        [ObservableProperty]
        private string generalStatusText;

        [ObservableProperty]
        private string generalStatusDescription;

        [ObservableProperty]
        private bool isRefreshing;

        // --- Listelerimiz ---
        public ObservableCollection<AnalysisCard> AnalysisCards { get; set; } = new();
        public ObservableCollection<CriticalAction> CriticalActions { get; set; } = new();

        public PassAnalyzerViewModel(IDatabaseService databaseService, ICryptoService cryptoService)
        {
            _databaseService = databaseService;
            _cryptoService = cryptoService;

            // Sayfa açıldığında analizi başlat
            IsRefreshing = true;
        }

        [RelayCommand]
        private async Task RunAnalysisAsync()
        {
            try
            {
                // 1. Ana şifreyi al ve veritabanından şifrelenmiş verileri çek
                master_pass = await SecureStorage.GetAsync("master_pass");
                var encryptedData = await _databaseService.GetDatabase();

                if (encryptedData == null || !encryptedData.Any())
                {
                    ResetToEmptyState();
                    return;
                }

                // Listeleri temizle
                CriticalActions.Clear();
                AnalysisCards.Clear();

                int strongCount = 0;
                int weakCount = 0;
                int riskyCount = 0;

                // 2. Şifrelerin analiz edilebilmesi için DECRYPT edilmiş hallerini geçici bir listede tutuyoruz
                var decryptedList = new List<(Password Original, string PlainText)>();
                foreach (var pwd in encryptedData)
                {
                    // SafeViewModel'deki gibi çözüyoruz şifreyi
                    string plainText = _cryptoService.Decrypt(pwd.EncryptedPassword, master_pass); // Modelindeki property ismi EncryptedPassword değilse düzelt reis
                    decryptedList.Add((pwd, plainText));
                }

                // 3. Tekrar eden şifreleri bulmak için düz metinleri grupluyoruz
                var passwordCounts = decryptedList.GroupBy(x => x.PlainText).ToDictionary(g => g.Key, g => g.Count());

                // 4. Analiz Döngüsü
                foreach (var item in decryptedList)
                {
                    bool isWeak = false;
                    bool isRisky = false;

                    // Kriter A: Zayıf Şifre Kontrolü (8 karakterden kısa veya çok basitse)
                    if (item.PlainText.Length < 8 || item.PlainText == "123456" || item.PlainText == "12345678")
                    {
                        weakCount++;
                        isWeak = true;

                        CriticalActions.Add(new CriticalAction
                        {
                            Title = item.Original.Title, // Modelindeki başlık alanı (Örn: Google, Trendyol)
                            Description = $"Çok zayıf şifre: \"{item.PlainText}\"",
                            IconKey = MauiIcons.Material.Sharp.MaterialSharpIcons.Warning.ToString(),
                            Color = "#FF5252", // Kırmızı
                            TargetPassword = item.Original
                        });
                    }

                    // Kriter B: Riskli Şifre Kontrolü (Aynı şifre birden fazla yerde varsa)
                    if (passwordCounts.TryGetValue(item.PlainText, out int count) && count > 1)
                    {
                        riskyCount++;
                        isRisky = true;

                        // Aynı hesaba ait hem zayıf hem tekrar eden uyarısı varsa mükerrer basmasın diye kontrol
                        if (!CriticalActions.Any(a => a.Title == item.Original.Title && a.Description.Contains("Tekrar eden")))
                        {
                            CriticalActions.Add(new CriticalAction
                            {
                                Title = item.Original.Title,
                                Description = "Tekrar eden şifre kullanılıyor",
                                IconKey = MauiIcons.Material.Sharp.MaterialSharpIcons.ContentCopy.ToString(),
                                Color = "#FFAB40", // Turuncu
                                TargetPassword = item.Original
                            });
                        }
                    }

                    // Kriter C: İkisine de takılmadıysa Güçlüdür temizdir
                    if (!isWeak && !isRisky)
                    {
                        strongCount++;
                    }
                }

                // 5. Üst İstatistik Kartlarını Bağla
                AnalysisCards.Add(new AnalysisCard { Title = "Güçlü", Count = strongCount, SideColor = "#20E19B", Description = "Karmaşık ve uzun karakterli güvenli şifreler.", IconKey = MauiIcons.Material.Sharp.MaterialSharpIcons.VerifiedUser.ToString() });
                AnalysisCards.Add(new AnalysisCard { Title = "Zayıf", Count = weakCount, SideColor = "#FF5252", Description = "8 karakterden kısa veya çok basit şifreler.", IconKey = MauiIcons.Material.Sharp.MaterialSharpIcons.ReportProblem.ToString() });
                AnalysisCards.Add(new AnalysisCard { Title = "Riskli", Count = riskyCount, SideColor = "#FFAB40", Description = "Birden fazla hesapta tekrar eden şifreler.", IconKey = MauiIcons.Material.Sharp.MaterialSharpIcons.ContentCopy.ToString() });

                // 6. Genel Skoru Hesapla
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