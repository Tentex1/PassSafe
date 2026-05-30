
using PassSafe.ViewModels;

namespace PassSafe.Views;

public partial class AddPasswordPopup
{
	public AddPasswordPopup(AddPasswordViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}