namespace PassSafe.ViewModels
{
    using CommunityToolkit.Mvvm.ComponentModel;
    using CommunityToolkit.Mvvm.Input;
    using PassSafe.Services;
    using PassSafe.Views;

    /// <summary>
    /// Defines the <see cref="ShellViewModel" />
    /// </summary>
    public partial class ShellViewModel : ObservableObject
    {
        [ObservableProperty]
        private object currentView;

        private readonly VaultView vaultView = new();

        private readonly PassGeneratorView passGeneratorView = new();

        private readonly PassAnalyzerView passAnalyzerView = new();

        private readonly SettingsView settingsView = new();

        public IDialogService _dialogService;
        public ShellViewModel(IDialogService dialogService)
        {
            CurrentView = vaultView;
            _dialogService = dialogService;
        }

        [RelayCommand]
        private void ChangeTab(string tabName)
        {
            CurrentView = tabName switch
            {
                "vault" => vaultView,
                "passgenerator" => passGeneratorView,
                "passanalyzer" => passAnalyzerView,
                "settings" => settingsView,
                _ => vaultView
            };
            _dialogService.ShowPopup(new SetMasterPassPopup());
        }
    }
}
