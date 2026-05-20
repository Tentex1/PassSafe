using PassSafe.ViewModels;

namespace PassSafe.Views;

public partial class VaultView : ContentView
{
	public VaultView()
	{
		InitializeComponent();

        Loaded += (s, e) =>
        {
            if (Window?.Page is NavigationPage navPage)
            {
                navPage.CurrentPage.Title = "Vault";
            }
            else if (App.Current.MainPage is NavigationPage mainNav)
            {
                mainNav.CurrentPage.Title = "Vault";
            }
        };
    }
}