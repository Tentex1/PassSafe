namespace PassSafe
{
    using CommunityToolkit.Maui;
    using Mopups.Hosting;
    using PassSafe.Services;
    using PassSafe.ViewModels;
    using PassSafe.Views;
    using Plugin.Maui.Biometric;
    using UraniumUI;

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
                .ConfigureMopups()
                .UseUraniumUI()
                .UseMauiCommunityToolkit()
                .UseUraniumUIMaterial()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                    fonts.AddMaterialSymbolsFonts();
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

            // Views / Pages / Popups
            builder.Services.AddSingleton<MainShell>();
            builder.Services.AddSingleton<SettingsView>();
            builder.Services.AddSingleton<PassAnalyzerView>();
            builder.Services.AddSingleton<PassGeneratorView>();
            builder.Services.AddSingleton<SafeView>();
            builder.Services.AddTransient<SetMasterPassPopup>();
            builder.Services.AddTransient<AddPasswordPopup>();
            return builder.Build();
        }
    }
}
