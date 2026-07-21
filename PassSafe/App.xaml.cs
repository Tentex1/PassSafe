using System.Drawing;
using System.Reflection;
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
            UserAppTheme = PlatformAppTheme;
            Services = serviceProvider;
            Color systemColor = null;
#if ANDROID
            systemColor = PlatformThemeHelper.GetAndroidAccentColor();
#elif WINDOWS
            systemColor = PassSafe.WinUI.PlatformThemeHelper.GetWindowsAccentColor();
#endif

            if (systemColor != null)
            {
                bool found = false;

                foreach (var dictionary in Application.Current.Resources.MergedDictionaries)
                {
                    if (dictionary.ContainsKey("Primary"))
                    {
                        dictionary["Primary"] = systemColor;

                        if (dictionary.ContainsKey("PrimaryBrush"))
                        {
                            dictionary["PrimaryBrush"] = new SolidColorBrush(systemColor);
                        }

                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    Application.Current.Resources["Primary"] = systemColor;
                }
            }

        }
        public static IDictionary<string, Type> Routes => _routes;
    }
}
