using PassSafe.ViewModels;

namespace PassSafe.Views;

public partial class MainShell
{
    public MainShell(ShellViewModel vm)
    {
        InitializeComponent();

        Loaded += async (s, e) =>
        {
            vm.InitializeCommand.ExecuteAsync(null);
        };

    }
}
