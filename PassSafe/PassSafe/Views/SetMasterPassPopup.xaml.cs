namespace PassSafe.Views;

public partial class SetMasterPassPopup
{
	public SetMasterPassPopup()
	{
		InitializeComponent();
	}

    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}