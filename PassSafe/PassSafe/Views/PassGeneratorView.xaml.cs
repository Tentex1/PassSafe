namespace PassSafe.Views;

public partial class PassGeneratorView : ContentView
{
	public PassGeneratorView()
	{
		InitializeComponent();

        Loaded += (s, e) =>
        {
            if (Window?.Page is NavigationPage navPage)
            {
                navPage.CurrentPage.Title = "Password Generator";
            }
            else if (App.Current.MainPage is NavigationPage mainNav)
            {
                mainNav.CurrentPage.Title = UraniumUI.Icons.MaterialSymbols.MaterialSharp.Password + " Password Generator";
            }
        };
    }
}