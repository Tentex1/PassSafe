namespace PassSafe.Services
{
    using Mopups.Pages;
    using Mopups.Services;
    using System.Threading.Tasks; // Task kullanabilmek için gerekli

    /// <summary>
    /// Defines the <see cref="DialogService" />
    /// </summary>
    public class DialogService : IDialogService
    {
        public Task ShowAlertAsync(string title, string message, string cancel)
        {
            return Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        public Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel)
        {
            return Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }

        public async Task ShowPopup(PopupPage popup)
        {
            await MopupService.Instance.PushAsync(popup);
        }
    }
}