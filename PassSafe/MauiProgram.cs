using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Layouts;
using MauiIcons.Material.Sharp;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;
using Mopups.Hosting;
using Plugin.Maui.Biometric;
using VijayAnand.MauiToolkit;

namespace PassSafe
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App, MainWindow, MainPage>();
            builder
                .UseMauiCommunityToolkit()
                .ConfigureMopups()
                .UseMaterialSharpMauiIcons()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-SemiBold.ttf", "OpenSansSemiBold");
                });

            // Services
            builder.Services.AddSingleton<IDatabaseService, DatabaseService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();
            builder.Services.AddSingleton<ICryptoService, AesCryptoService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddSingleton((e) => BiometricAuthenticationService.Default);

            // ViewModels
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<SetMasterPassViewModel>();
            builder.Services.AddTransient<ImportDatabaseVerifyViewModel>();
            builder.Services.AddTransient<AddPasswordViewModel>();
            builder.Services.AddSingleton<SafeViewModel>();
            builder.Services.AddTransient<PassGeneratorViewModel>();
            builder.Services.AddTransient<PassAnalyzerViewModel>();

            // Views / Pages / Popups
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<PassAnalyzerPage>();
            builder.Services.AddTransient<PassGeneratorPage>();
            builder.Services.AddTransient<SafePage>();
            builder.Services.AddTransient<SetMasterPassPopup>();
            builder.Services.AddTransient<ImportDatabaseVerifyPopup>();
            builder.Services.AddTransient<AddPasswordPopup>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

#if WINDOWS
            // Launch the app window maximized on Windows
            builder.ConfigureLifecycleEvents(events =>
            {
                events.AddWindows(app =>
                {
                    app.OnWindowCreated(window =>
                    {
                        window.ExtendsContentIntoTitleBar = false;

                        if (window.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                        {
                            //presenter.SetBorderAndTitleBar(false, false);
                            presenter.Maximize();
                        }
                    });
                });
            });
#endif

            return builder.Build();
        }
    }
}
