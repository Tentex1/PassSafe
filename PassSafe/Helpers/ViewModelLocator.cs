namespace PassSafe.Helpers
{
    using PassSafe.ViewModels;

    /// <summary>
    /// Defines the <see cref="ViewModelLocator" />
    /// </summary>
    public class ViewModelLocator
    {
        public MainViewModel MainViewModel => App.Services.GetService<MainViewModel>();
        public SettingsViewModel SettingsViewModel => App.Services.GetService<SettingsViewModel>();
        public SetMasterPassViewModel SetMasterPassViewModel => App.Services.GetService<SetMasterPassViewModel>();
        public ImportDatabaseVerifyViewModel ImportDatabaseVerifyViewModel => App.Services.GetService<ImportDatabaseVerifyViewModel>();
        public AddPasswordViewModel AddPasswordViewModel => App.Services.GetService<AddPasswordViewModel>();

        public SafeViewModel SafeViewModel => App.Services.GetService<SafeViewModel>();

        public PassGeneratorViewModel PassGeneratorViewModel => App.Services.GetService<PassGeneratorViewModel>();

        public PassAnalyzerViewModel PassAnalyzerViewModel => App.Services.GetService<PassAnalyzerViewModel>();
    }
}
