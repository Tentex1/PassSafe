using PassSafe.Resources;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace PassSafe.Helpers
{
    public class LocalizationManager : INotifyPropertyChanged
    {
        public static LocalizationManager Instance { get; } = new LocalizationManager();

        // Dil değiştiğinde XAML'deki Binding'lerin tetiklenmesi için indexer kullanıyoruz
        public string this[string resourceKey] =>
            AppResources.ResourceManager.GetString(resourceKey, AppResources.Culture) ?? resourceKey;

        public void SetLanguage(string lang)
        {
            var culture = new CultureInfo(lang);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            AppResources.Culture = culture;

            // CRITICAL: Bu satır arayüzdeki tüm {Binding Loc[Key], Source=...} yapılarını uyarır
            OnPropertyChanged(null); // null veya string.Empty tüm propertyleri yeniler
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}