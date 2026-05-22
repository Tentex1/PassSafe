namespace PassSafe.Views;

public partial class SafeView : ContentView
{
	public SafeView()
	{
		InitializeComponent();

        Loaded += (s, e) =>
        {
            if (Window?.Page is NavigationPage navPage)
            {
                navPage.CurrentPage.Title = "Password Safe";
            }
            else if (App.Current.MainPage is NavigationPage mainNav)
            {
                mainNav.CurrentPage.Title = "Password Safe";
            }
        };
    }
}