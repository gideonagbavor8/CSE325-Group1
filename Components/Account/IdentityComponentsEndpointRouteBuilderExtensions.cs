using ChefConnect.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChefConnect.Components.Account;

internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
    // Non-Blazor endpoints required by Identity pages (Just Logout atm).
    // Logout must be a minimal API endpoint, not a Blazor component so it needs to
    // write to the response (clear the auth cookie) after headers (may) already be
    // sent, which Blazor can't do reliably.
    public static IEndpointRouteBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup("/Account");

        accountGroup.MapPost("/Logout", async (ClaimsPrincipal user, SignInManager<ApplicationUser> signInManager, [FromForm] string returnUrl) =>
            {
                await signInManager.SignOutAsync();
                return TypedResults.LocalRedirect($"~/{returnUrl}");
            }
        );

        return endpoints;
    }
}
