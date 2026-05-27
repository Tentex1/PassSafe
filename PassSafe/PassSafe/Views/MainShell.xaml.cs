using PassSafe.ViewModels;

namespace PassSafe.Views;

public partial class MainShell : UraniumUI.Pages.UraniumContentPage
{
    public MainShell()
    {
        InitializeComponent();

        if(BindingContext is ShellViewModel vm)
        {
            vm.InitializeCommand.ExecuteAsync(null);
        }
    }
}
