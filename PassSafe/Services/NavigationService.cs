namespace PassSafe.Services
{
    using Microsoft.Maui.Controls;
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Implementation of the navigation service. 
    /// Handles routing and pushing/popping pages to the navigation stack.
    /// </summary>
    public partial class NavigationService(IServiceProvider services) : INavigationService
    {
        /// <summary>
        /// Navigates to the specified registered route.
        /// </summary>
        public Task PushAsync(string route)
        {
            if (!App.Routes.TryGetValue(route, out Type? type))
                throw new RouteNotFoundException();

            if (services.GetService(type) is not Page page)
                throw new TypeNotRegisteredException();

            var root = Application.Current?.Windows?[0]?.Page;

            return root switch
            {
                not null => root.Navigation.PushAsync(page),
                _ => throw new InvalidOperationException("Window's Page cannot be null.")
            };
        }

        /// <summary>
        /// Returns to the previous page in the navigation stack.
        /// </summary>
        public Task PopAsync()
        {
            var root = Application.Current?.Windows?[0]?.Page;

            return root switch
            {
                not null => root.Navigation.PopAsync(),
                _ => throw new InvalidOperationException("Window's Page cannot be null.")
            };
        }

        /// <summary>
        /// Returns to the root (main) page of the application.
        /// </summary>
        public Task PopToRootAsync()
        {
            var root = Application.Current?.Windows?[0]?.Page;

            return root switch
            {
                not null => root.Navigation.PopToRootAsync(),
                _ => throw new InvalidOperationException("Window's Page cannot be null.")
            };
        }

        /// <summary>
        /// Opens a page modally (blocks the underlying UI).
        /// </summary>
        public Task PushModalAsync(string route)
        {
            if (!App.Routes.TryGetValue(route, out Type? type))
                throw new RouteNotFoundException();

            if (services.GetService(type) is not Page page)
                throw new TypeNotRegisteredException();

            var root = Application.Current?.Windows?[0]?.Page;

            return root switch
            {
                not null => root.Navigation.PushModalAsync(page),
                _ => throw new InvalidOperationException("Window's Page cannot be null.")
            };
        }

        /// <summary>
        /// Closes the currently active modal page.
        /// </summary>
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

    public class RouteNotFoundException : Exception { }
    public class TypeNotRegisteredException : Exception { }
}