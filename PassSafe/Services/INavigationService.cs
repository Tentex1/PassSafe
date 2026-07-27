namespace PassSafe.Services
{
    /// <summary>
    /// Defines the <see cref="INavigationService" />
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
