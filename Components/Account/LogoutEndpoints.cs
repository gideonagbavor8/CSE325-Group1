using Microsoft.AspNetCore.Identity;

namespace ChefConnect.Components.Account;

public static class LogoutEndpoint
{
    public static void MapLogoutEndpoint(this WebApplication app)
    {
        app.MapPost("/account/logout",
            async (SignInManager<ApplicationUser> signInManager) =>
            {
                await signInManager.SignOutAsync();

                return Results.Redirect("/login");
            });
    }
}