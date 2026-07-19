namespace PassSafe.Services
{
    public interface INavigationService
    {
        Task PushAsync(string route);

        Task PopAsync();

        Task PopToRootAsync();

        Task PushModalAsync(string route);

        Task PopModalAsync();
    }
}
