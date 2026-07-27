using PassSafe.Resources;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace PassSafe.Helpers
{
    /// <summary>
    /// Manages real-time language translations across the app without requiring a restart.
    /// </summary>
    public class LocalizationManager : INotifyPropertyChanged
    {
        public static LocalizationManager Instance { get; } = new LocalizationManager();

        /// <summary>
        /// Indexer used to trigger XAML bindings instantly when the language changes.
        /// </summary>
        public string this[string resourceKey] =>
            AppResources.ResourceManager.GetString(resourceKey, AppResources.Culture) ?? resourceKey;

        /// <summary>
        /// Updates the current culture of the app and notifies all UI elements to refresh their texts.
        /// </summary>
        /// <param name="lang">The two-letter language code (e.g., "en", "tr", "ru").</param>
        public void SetLanguage(string lang)
        {
            var culture = new CultureInfo(lang);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            AppResources.Culture = culture;

            // CRITICAL: This line notifies all {Binding Loc[Key]} structures in the UI to refresh instantly.
            OnPropertyChanged(null);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}