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
        public async Task ShowAlertAsync(string title, string message, string cancel)
        {
            await Application.Current.MainPage.DisplayAlertAsync(title, message, cancel);
        }

        public async Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel)
        {
            return await Application.Current.MainPage.DisplayAlertAsync(title, message, accept, cancel);
        }
        public async Task ShowErrorAsync(string title, Exception ex, string accept, string cancel)
        {
            var dialog = await Application.Current.MainPage.DisplayAlertAsync(title, ex.Message, accept, cancel);

            if (dialog == true) { await Clipboard.Default.SetTextAsync(ex.Message); System.Diagnostics.Debug.WriteLine("blok bitti"); }
            else { return; }
        }

        public async Task ShowPopup(PopupPage popup)
        {
            await MopupService.Instance.PushAsync(popup);
        }
    }
}