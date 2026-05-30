using PassSafe.ViewModels;
using PassSafe.Views;

namespace PassSafe
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; set; }

        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            Services = serviceProvider;

            // Konteynerden ShellViewModel'i çözümlüyoruz (resolve)
            var viewModel = serviceProvider.GetRequiredService<ShellViewModel>();

            MainPage = new NavigationPage(new MainShell(viewModel));
        }
    }
}
