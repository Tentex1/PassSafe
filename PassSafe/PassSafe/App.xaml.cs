using PassSafe.Views;
using UraniumUI.Material.Resources;

namespace PassSafe
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; set; }
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            Services = serviceProvider;
            MainPage = new NavigationPage(new MainShell());
        }
    }
}
