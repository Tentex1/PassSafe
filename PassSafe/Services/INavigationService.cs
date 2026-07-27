namespace PassSafe.Services
{
    using System.Threading.Tasks;

    /// <summary>
    /// Provides methods to navigate between pages.
    /// </summary>
    public interface INavigationService
    {
        Task PushAsync(string route);
        Task PopAsync();
        Task PopToRootAsync();
        Task PushModalAsync(string route);
        Task PopModalAsync();
    }
}