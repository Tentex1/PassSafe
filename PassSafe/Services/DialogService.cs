namespace PassSafe.Services
{
    using Microsoft.Maui.ApplicationModel.DataTransfer;
    using Microsoft.Maui.Controls;
    using Mopups.Pages;
    using Mopups.Services;
    using PassSafe.Helpers;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation of the Dialog Service. Acts as a wrapper for MAUI's native display alerts and Mopups.
    /// </summary>
    public class DialogService : IDialogService
    {
        private Page mainPage => Application.Current?.Windows.FirstOrDefault()?.Page;

        /// <summary>
        /// Displays a simple native alert dialog with a single dismiss button.
        /// </summary>
        public async Task ShowAlertAsync(string title, string message, string cancel)
        {
            await mainPage.DisplayAlertAsync(title, message, cancel);
        }

        /// <summary>
        /// Displays a confirmation dialog with accept and cancel buttons. Returns true if accepted.
        /// </summary>
        public async Task<bool> ShowConfirmAsync(string title, string message, string accept, string cancel)
        {
            return await mainPage.DisplayAlertAsync(title, message, accept, cancel);
        }

        /// <summary>
        /// Displays an error dialog with an option to copy the exception message to the clipboard.
        /// </summary>
        public async Task ShowErrorAsync(Exception ex = null, string message = null)
        {
            var text = ex != null ? ex.Message : message;

            // Replaced hardcoded Turkish words with Localization keys
            var loc = LocalizationManager.Instance;
            var dialog = await mainPage.DisplayAlertAsync(loc["ErrorTitle"], text, "Copy", loc["OkBtn"]);

            if (dialog == true && ex != null)
            {
                await Clipboard.Default.SetTextAsync(ex.Message);
            }
        }

        /// <summary>
        /// Displays a native prompt dialog requesting text input from the user.
        /// </summary>
        public async Task<string> ShowPromptAsync(string title, string message, string accept, string cancel)
        {
            return await mainPage.DisplayPromptAsync(title, message, accept, cancel);
        }

        /// <summary>
        /// Pushes a custom Mopups page to the UI stack.
        /// </summary>
        public async Task ShowPopupAsync(PopupPage popup)
        {
            await MopupService.Instance.PushAsync(popup);
        }
    }
}