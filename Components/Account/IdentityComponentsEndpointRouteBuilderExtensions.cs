/*
 * ===========================================================
 * ChefConnect - CSE 325 Group Project
 *
 * File: IdentityComponentsEndpointRouteBuilderExtensions.cs
 *
 * Purpose:
 * Provides additional ASP.NET Core Identity endpoints that
 * cannot be handled directly by Blazor components.
 *
 * The logout endpoint signs the current user out and safely
 * redirects the user to a local page afterward.
 * ===========================================================
 */

using ChefConnect.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChefConnect.Components.Account;

/// <summary>
/// Provides additional endpoints required by the ChefConnect
/// Identity system.
/// </summary>
internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps additional Identity endpoints used by the application.
    ///
    /// Logout is implemented as a minimal API endpoint because
    /// clearing the authentication cookie requires modifying
    /// the HTTP response.
    /// </summary>
    public static IEndpointRouteBuilder MapAdditionalIdentityEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        var accountGroup = endpoints.MapGroup("/Account");

        // =====================================================
        // Logout
        // =====================================================
        //
        // The logout request is sent using POST from Navbar.razor.
        // ASP.NET Core Identity clears the authentication cookie,
        // then the user is redirected to a safe local URL.
        //
        accountGroup.MapPost(
            "/Logout",
            async (
                ClaimsPrincipal user,
                SignInManager<ApplicationUser> signInManager,
                [FromForm] string? returnUrl) =>
            {
                // Sign the current user out and clear the
                // authentication cookie.
                await signInManager.SignOutAsync();

                // Use the supplied return URL only when it is
                // a valid local URL.
                if (!string.IsNullOrWhiteSpace(returnUrl) &&
                    returnUrl.StartsWith("/") &&
                    !returnUrl.StartsWith("//") &&
                    !returnUrl.StartsWith("/\\"))
                {
                    return TypedResults.LocalRedirect(returnUrl);
                }

                // If the return URL is missing or invalid,
                // safely return the user to the home page.
                return TypedResults.LocalRedirect("/");
            });

        return endpoints;
    }
}