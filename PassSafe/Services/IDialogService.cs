namespace PassSafe.Services
{
    using Mopups.Pages;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Provides methods to display native alerts, prompts, and custom popups.
    /// </summary>
    public interface IDialogService
    {
        Task ShowAlertAsync(string title, string message, string cancel);
        Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel);
        Task ShowErrorAsync(Exception ex = null, string message = null);
        Task<string> ShowPromptAsync(string title, string message, string accept, string cancel);
        Task ShowPopupAsync(PopupPage popup);
    }
}