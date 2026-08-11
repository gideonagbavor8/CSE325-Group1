/*
 * ===========================================================
 * ChefConnect - CSE 325 Group Project
 *
 * File: Program.cs
 *
 * Contributors:
 *   - Godfred Sefa Aboagye
 *   - Kamohelo Godfrey Mejaele
 *
 * Purpose:
 * Configures the ChefConnect application at startup.
 * This file registers application services, configures the
 * Entity Framework Core database context, and defines the
 * application's HTTP request pipeline.
 * ===========================================================
 */

using ChefConnect.Components;
using ChefConnect.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ChefConnect.Components.Account;
using ChefConnect.Services;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.FileProviders;
using System.Linq.Expressions;

var builder = WebApplication.CreateBuilder(args);

//
// ===========================================================
// Register the ChefConnect database context.
//
// This enables Entity Framework Core to communicate with the
// SQL Server database using the connection string stored in
// appsettings.json.
//
// Dependency Injection allows the database context to be
// accessed throughout the application whenever needed.
// ===========================================================
//
builder.Services.AddScoped<RecipeService>();

// 1. Add Database Connection (SQLite)
var defaultConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=app.db";

//
// When hosted, the database file is kept outside the application
// folder so that it survives a redeployment. SQLite creates the
// file it is pointed at, but not the folder holding it, so the
// folder is created here before Entity Framework opens it.
//
var sqliteDataSource = new SqliteConnectionStringBuilder(defaultConnectionString).DataSource;

if (!string.IsNullOrWhiteSpace(sqliteDataSource))
{
    var databaseDirectory = Path.GetDirectoryName(Path.GetFullPath(sqliteDataSource));

    if (!string.IsNullOrEmpty(databaseDirectory))
    {
        Directory.CreateDirectory(databaseDirectory);
    }
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(defaultConnectionString));

builder.Services.AddIdentityCore<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;

    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddSignInManager()
.AddDefaultTokenProviders();
//
// ===========================================================
// Register Blazor Server services.
//
// These services enable Razor Components and interactive
// server-side rendering for the ChefConnect application.
// ===========================================================
//
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

builder.Services.AddAuthorizationCore();
//
// ===========================================================
// Build the application.
//
// This creates the WebApplication instance after all required
// services have been registered.
// ===========================================================
//
var app = builder.Build();

//
// ===========================================================
// Configure the HTTP request pipeline.
//
// The following middleware controls how incoming requests are
// processed before reaching the application.
// ===========================================================
//

// Display a custom error page when running in Production.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);

    // Enables HTTP Strict Transport Security (HSTS)
    // to improve application security.
    app.UseHsts();
}

// Display a custom page whenever a resource cannot be found.
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// Redirect all HTTP requests to HTTPS.
app.UseHttpsRedirection();

// Serve CSS, JavaScript, images, and other static assets.
app.MapStaticAssets();

//
// ===========================================================
// Serve uploaded recipe images.
//
// MapStaticAssets only serves the files that existed when the
// application was built, so the uploads folder is served
// separately. This allows an image uploaded by a user to be
// displayed immediately without restarting the application.
// ===========================================================
//
// When hosted, RecipeImages:StoragePath points at a folder that
// survives redeployment. Locally the setting is absent and images
// stay in wwwroot as before.
RecipeImageService.UseStorageDirectory(
    builder.Configuration["RecipeImages:StoragePath"]);

var recipeUploadsPath = RecipeImageService.EnsureUploadsDirectory(app.Environment);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(recipeUploadsPath),
    RequestPath = RecipeImageService.UploadsRequestPath
});

//
// ===========================================================
// Map the root Razor component.
//
// This tells ASP.NET Core to start the application using
// the App component and enables interactive server rendering.
// ===========================================================
//
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();

//
// ===========================================================
// Start the ChefConnect application.
//
// After this point the application begins listening for
// incoming HTTP requests.
// ===========================================================
//
app.UseAuthentication();
app.UseAuthorization();

// Protect against Cross-Site Request Forgery (CSRF) attacks.
// Called once, the UseAntiforgery() call earlier
// caused the form to be read twice, which threw
// "invalid anti-forgery token" on every POST.
app.UseAntiforgery();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await DbInitializer.InitializeAsync(context, userManager);
}

app.Run();