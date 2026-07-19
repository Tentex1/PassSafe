using Android.App;
using Android.Content.PM;
using Android.OS;

namespace PassSafe
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : MauiAppCompatActivity
    {
        
    }
    public static class PlatformThemeHelper
    {
        public static Color GetAndroidAccentColor()
        {
            var context = Android.App.Application.Context;

            // Android 12 (API 31) ve üzeri için dinamik Material You rengi
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.S)
            {
                int resourceId = Android.Resource.Color.SystemAccent1500; // Ana vurgu rengi
                int colorInt = context.GetColor(resourceId);
                return Color.FromInt(colorInt);
            }

            // Eski Android sürümleri için uygulamanın kendi theme accent'ini düşebilirsin
            var typedValue = new Android.Util.TypedValue();
            if (context.Theme.ResolveAttribute(Android.Resource.Attribute.ColorAccent, typedValue, true))
            {
                return Color.FromInt(typedValue.Data);
            }

            return Color.FromArgb("#512BD4"); // Fallback
        }
    }
}
