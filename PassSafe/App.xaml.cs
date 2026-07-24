using System.Drawing;
using System.Reflection;
using Microsoft.Maui.Storage;
using PassSafe.Helpers;     
using Color = Microsoft.Maui.Graphics.Color;

namespace PassSafe
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; set; }

        private static readonly Dictionary<string, Type> _routes = new()
        {
            ["safe"] = typeof(SafePage),
            ["passGenerator"] = typeof(PassGeneratorPage),
            ["passAnalyzer"] = typeof(PassAnalyzerPage),
            ["settings"] = typeof(SettingsPage)
        };

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            Services = serviceProvider;

            string savedLang = Preferences.Get("AppLanguage", "en");   
            LocalizationManager.Instance.SetLanguage(savedLang);

            string savedTheme = Preferences.Get("AppTheme", "system");
            if (savedTheme == "light")
                UserAppTheme = AppTheme.Light;
            else if (savedTheme == "dark")
                UserAppTheme = AppTheme.Dark;
            else
                UserAppTheme = AppTheme.Unspecified;
        }

        public static IDictionary<string, Type> Routes => _routes;
    }
}