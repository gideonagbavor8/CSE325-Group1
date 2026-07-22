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
builder.Services.AddDbContext<ChefConnectContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("ChefConnectContext")));

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

// Protect against Cross-Site Request Forgery (CSRF) attacks.
app.UseAntiforgery();

// Serve CSS, JavaScript, images, and other static assets.
app.MapStaticAssets();

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

//
// ===========================================================
// Start the ChefConnect application.
//
// After this point the application begins listening for
// incoming HTTP requests.
// ===========================================================
//
app.Run();