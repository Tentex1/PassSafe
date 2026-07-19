namespace PassSafe.Services
{
    using Mopups.Pages;

    /// <summary>
    /// Defines the <see cref="IDialogService" />
    /// </summary>
    public interface IDialogService
    {
        Task ShowAlertAsync(string title, string message, string cancel);

        Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel);

        Task ShowErrorAsync(Exception ex);

        Task ShowPopupAsync(PopupPage popup);
    }
}
