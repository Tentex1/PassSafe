namespace PassSafe.Services
{
    using Mopups.Pages;
    using Mopups.Services;
    using System.Threading.Tasks;     

    public class DialogService : IDialogService
    {
        private Page mainPage => Application.Current?.Windows.FirstOrDefault()?.Page;
        public async Task ShowAlertAsync(string title, string message, string cancel)
        {
            await mainPage.DisplayAlertAsync(title, message, cancel);
        }

        public async Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel)
        {
            return await mainPage.DisplayAlertAsync(title, message, accept, cancel);
        }

        public async Task ShowErrorAsync(Exception ex =null, string message = null)
        {
            var text = ex != null ? ex.Message : message;
            var dialog = await mainPage.DisplayAlertAsync("Hata", text, "Kopyala", "Tamam");

            if (dialog == true) { await Clipboard.Default.SetTextAsync(ex.Message); }
            else { return; }
        }

        public async Task ShowPopupAsync(PopupPage popup)
        {
            await MopupService.Instance.PushAsync(popup);
        }
    }
}
