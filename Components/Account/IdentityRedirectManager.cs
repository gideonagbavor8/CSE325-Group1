using Microsoft.AspNetCore.Components;

namespace ChefConnect.Components.Account;

// Helper used by the Identity pages (Login/Register) to issue redirects
// and to pass one-time status messages across a redirect through a short lived cookie.
//
// .NET 10's .csproj <BlazorDisableThrowNavigationException> is true by default so
// NavigateTo() no longer throws to halt execution, so [DoesNotReturn] -> throw
// exception pattern was removed. RedirectTo just calls NavigateTo() and returns.
internal sealed class IdentityRedirectManager(NavigationManager navigationManager)
{
    public const string StatusCookieName = "Identity.StatusMessage";

    private static readonly CookieBuilder StatusCookieBuilder = new()
    {
        SameSite = SameSiteMode.Strict,
        HttpOnly = true,
        IsEssential = true,
        MaxAge = TimeSpan.FromSeconds(5),
    };

    public void RedirectTo(string? uri)
    {
        uri ??= "";

        // Prevent open redirects
        if (!Uri.IsWellFormedUriString(uri, UriKind.Relative))
        {
            uri = navigationManager.ToBaseRelativePath(uri);
        }

        navigationManager.NavigateTo(uri, forceLoad: true);
    }

    public void RedirectTo(string uri, Dictionary<string, object?> queryParameters)
    {
        var uriWithoutQuery = navigationManager.ToAbsoluteUri(uri).GetLeftPart(UriPartial.Path);
        var newUri = navigationManager.GetUriWithQueryParameters(uriWithoutQuery, queryParameters);
        RedirectTo(newUri);
    }

    public void RedirectToWithStatus(string uri, string message, HttpContext context)
    {
        context.Response.Cookies.Append(StatusCookieName, message, StatusCookieBuilder.Build(context));
        RedirectTo(uri);
    }

    private string CurrentPath => navigationManager.ToAbsoluteUri(navigationManager.Uri).GetLeftPart(UriPartial.Path);
    public void RedirectToCurrentPage() => RedirectTo(CurrentPath);
    public void RedirectToCurrentPageWithStatus(string message, HttpContext context) => RedirectToWithStatus(CurrentPath, message, context);
}
