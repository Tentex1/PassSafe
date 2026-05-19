namespace PassSafe
{
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
                .UseUraniumUIMaterial()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                    fonts.AddMaterialSymbolsFonts();
                });

            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<ICryptoService, AesCryptoService>();
            builder.Services.AddSingleton<ShellViewModel>();
            builder.Services.AddSingleton<MainShell>();
            builder.Services.AddSingleton<SettingsView>();
            builder.Services.AddSingleton<PassAnalyzerView>();
            builder.Services.AddSingleton<PassGeneratorView>();
            builder.Services.AddSingleton<VaultView>();
            builder.Services.AddSingleton<VaultViewModel>();
            builder.Services.AddSingleton<SetMasterPassPopup>();
            builder.Services.AddSingleton<AddPasswordPopup>();
            builder.Services.AddSingleton<SetMasterPassViewModel>();
            builder.Services.AddSingleton<AddPasswordViewModel>();
            builder.Services.AddSingleton((e) => BiometricAuthenticationService.Default);
            return builder.Build();
        }
    }
}
