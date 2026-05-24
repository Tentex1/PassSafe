namespace PassSafe.Helpers
{
    using PassSafe.ViewModels;

    /// <summary>
    /// Defines the <see cref="ViewModelLocator" />
    /// </summary>
    public class ViewModelLocator
    {
        public ShellViewModel ShellViewModel => App.Services.GetService<ShellViewModel>();

        public SetMasterPassViewModel SetMasterPassViewModel => App.Services.GetService<SetMasterPassViewModel>();

        public AddPasswordViewModel AddPasswordViewModel => App.Services.GetService<AddPasswordViewModel>();

        public SafeViewModel SafeViewModel => App.Services.GetService<SafeViewModel>();

        public PassGeneratorViewModel PassGeneratorViewModel => App.Services.GetService<PassGeneratorViewModel>();
    }
}
