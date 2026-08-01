using Microsoft.AspNetCore.Identity;

namespace ChefConnect.Components.Account;


public static class LoginEndpoint
{

    public static void MapLoginEndpoint(this WebApplication app)
    {

        app.MapPost("/account/login",
        async (
            HttpContext context,
            SignInManager<ApplicationUser> signInManager) =>
        {

            var form = await context.Request.ReadFormAsync();


            var email = form["Email"].ToString();

            var password = form["Password"].ToString();



            var result = await signInManager.PasswordSignInAsync(
                email,
                password,
                false,
                false
            );


            if(result.Succeeded)
            {
                return Results.Redirect("/");
            }


            return Results.Redirect("/login?error=failed");

        });

    }

}