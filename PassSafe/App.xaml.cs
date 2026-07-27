namespace PassSafe
{
    using Microsoft.Maui.Controls;
    using Microsoft.Maui.Storage;
    using PassSafe.Helpers;
    using PassSafe.Views;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The main entry point of the application. 
    /// Handles global routing, theming, and localization initialization on startup.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Provides global access to the Dependency Injection (DI) container.
        /// </summary>
        public static IServiceProvider Services { get; set; }

        // Dictionary to store navigation routes mapped to their respective Views
        private static readonly Dictionary<string, Type> _routes = new()
        {
            ["safe"] = typeof(SafePage),
            ["passGenerator"] = typeof(PassGeneratorPage),
            ["passAnalyzer"] = typeof(PassAnalyzerPage),
            ["settings"] = typeof(SettingsPage)
        };

        /// <summary>
        /// Initializes the app, sets up language based on system or user preference, 
        /// and applies the selected Light/Dark theme.
        /// </summary>
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            Services = serviceProvider;

            // 1. AUTO-DETECT SYSTEM LANGUAGE AND LOAD PREFERENCE
            // Gets the two-letter language code of the device (e.g., "en", "tr", "ru")
            string sysLang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLower();

            // If the system is Turkish or Russian, use it. Otherwise, default to English.
            string defaultLang = (sysLang == "tr" || sysLang == "ru") ? sysLang : "en";

            // Load saved language or fallback to default
            string savedLang = Preferences.Get("AppLanguage", defaultLang);
            LocalizationManager.Instance.SetLanguage(savedLang);

            // 2. LOAD USER'S PREFERRED THEME
            string savedTheme = Preferences.Get("AppTheme", "system");
            if (savedTheme == "light")
                UserAppTheme = AppTheme.Light;
            else if (savedTheme == "dark")
                UserAppTheme = AppTheme.Dark;
            else
                UserAppTheme = AppTheme.Unspecified; // System Default
        }

        /// <summary>
        /// Exposes the routing dictionary for the Navigation Service.
        /// </summary>
        public static IDictionary<string, Type> Routes => _routes;
    }
}