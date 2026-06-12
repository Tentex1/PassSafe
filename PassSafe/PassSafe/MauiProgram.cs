namespace PassSafe
{
    using CommunityToolkit.Maui;
    using MauiIcons.Material.Sharp;
    using Mopups.Hosting;
    using PassSafe.Services;
    using PassSafe.ViewModels;
    using PassSafe.Views;
    using Plugin.Maui.Biometric;

    /// <summary>
    /// Defines the <see cref="MauiProgram" />
    /// </summary>
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMaterialSharpMauiIcons()
                .ConfigureMopups()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Services
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<ICryptoService, AesCryptoService>();
            builder.Services.AddSingleton((e) => BiometricAuthenticationService.Default);

            // ViewModels
            builder.Services.AddSingleton<ShellViewModel>();
            builder.Services.AddSingleton<SetMasterPassViewModel>();
            builder.Services.AddSingleton<AddPasswordViewModel>();
            builder.Services.AddSingleton<SafeViewModel>();
            builder.Services.AddSingleton<PassGeneratorViewModel>();
            builder.Services.AddSingleton<PassAnalyzerViewModel>();

            // Views / Pages / Popups
            builder.Services.AddTransient<MainShell>();
            builder.Services.AddTransient<SettingsView>();
            builder.Services.AddTransient<PassAnalyzerView>();
            builder.Services.AddTransient<PassGeneratorView>();
            builder.Services.AddTransient<SafeView>();
            builder.Services.AddTransient<SetMasterPassPopup>();
            builder.Services.AddTransient<AddPasswordPopup>();
            return builder.Build();
        }
    }
}
