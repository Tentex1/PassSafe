namespace PassSafe.Services
{
    public partial class NavigationService(IServiceProvider services) : INavigationService
    {
        public Task PushAsync(string route)
        {
            if (!App.Routes.TryGetValue(route, out Type? type))
            {
                throw new RouteNotFoundException();
            }

            if (services.GetService(type) is not Page page)
            {
                throw new TypeNotRegisteredException();
            }

            var root = Application.Current?.Windows?[0]?.Page;

            return root switch
            {
                not null => root.Navigation.PushAsync(page),
                _ => throw new InvalidOperationException("Window's Page cannot be null.")
            };
        }

        public Task PopAsync()
        {
            var root = Application.Current?.Windows?[0]?.Page;

            return root switch
            {
                not null => root.Navigation.PopAsync(),
                _ => throw new InvalidOperationException("Window's Page cannot be null.")
            };
        }

        public Task PopToRootAsync()
        {
            var root = Application.Current?.Windows?[0]?.Page;

            return root switch
            {
                not null => root.Navigation.PopToRootAsync(),
                _ => throw new InvalidOperationException("Window's Page cannot be null.")
            };
        }

        public Task PushModalAsync(string route)
        {
            if (!App.Routes.TryGetValue(route, out Type? type))
            {
                throw new RouteNotFoundException();
            }

            if (services.GetService(type) is not Page page)
            {
                throw new TypeNotRegisteredException();
            }

            var root = Application.Current?.Windows?[0]?.Page;

            return root switch
            {
                not null => root.Navigation.PushModalAsync(page),
                _ => throw new InvalidOperationException("Window's Page cannot be null.")
            };
        }

        public Task PopModalAsync()
        {
            var root = Application.Current?.Windows?[0]?.Page;

            return root switch
            {
                not null => root.Navigation.PopModalAsync(),
                _ => throw new InvalidOperationException("Window's Page cannot be null.")
            };
        }
    }
}
