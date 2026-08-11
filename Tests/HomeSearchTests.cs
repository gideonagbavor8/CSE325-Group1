using Bunit;
using ChefConnect.Components.Pages;
using ChefConnect.Components.Pages.Recipes;
using ChefConnect.Data;
using ChefConnect.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ChefConnect.Tests;

internal sealed class AnonymousAuthStateProvider : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(new AuthenticationState(
            new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity())));
}

/// <summary>
/// Reproduces the reported problem: favorite a recipe, then search
/// for it on the home page.
/// </summary>
public class HomeSearchTests : IDisposable
{
    private const string TestUserId = "test-user-1";

    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _db;
    private readonly TestContext _ctx = new();
    private readonly string _root;

    public HomeSearchTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options);

        _db.Database.EnsureCreated();

        _db.Categories.Add(new Category { Id = 1, Name = "Breakfast" });
        _db.Categories.Add(new Category { Id = 2, Name = "Dinner" });
        _db.Users.Add(new ApplicationUser
        {
            Id = TestUserId,
            UserName = "tester@example.com",
            NormalizedUserName = "TESTER@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString()
        });

        _db.Recipes.Add(NewRecipe(1, "Pancakes", 1));
        _db.Recipes.Add(NewRecipe(2, "Jollof Rice", 2));
        _db.Recipes.Add(NewRecipe(3, "Apple Pie", 2));
        _db.SaveChanges();
        _db.ChangeTracker.Clear();

        _root = Path.Combine(Path.GetTempPath(), "chefconnect-home-tests", Guid.NewGuid().ToString("N"));

        _ctx.Services.AddSingleton(_db);
        _ctx.Services.AddSingleton(new RecipeService(_db));
        _ctx.Services.AddSingleton<IWebHostEnvironment>(new FakeEnvironment(_root));
        _ctx.Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthStateProvider(TestUserId));
        _ctx.Services.AddAuthorizationCore();
        _ctx.Services.AddCascadingAuthenticationState();
    }

    private static Recipe NewRecipe(int id, string name, int categoryId) => new()
    {
        Id = id,
        Name = name,
        Description = "d",
        Ingredients = "i",
        Instructions = "n",
        PreparationTime = 5,
        CookingTime = 5,
        Servings = 2,
        CategoryId = categoryId,
        UserId = TestUserId
    };

    [Fact]
    public void Search_FindsRecipe_WhenNotFavorited()
    {
        var cut = _ctx.RenderComponent<Home>();

        cut.Find("#recipe-search").Input("Pancakes");

        Assert.Contains("Pancakes", cut.Markup);
        Assert.DoesNotContain("No recipes match your search", cut.Markup);
    }

    [Fact]
    public async Task Search_FindsRecipe_AfterItWasFavorited()
    {
        // Favorite the recipe exactly the way the detail page does.
        var service = new RecipeService(_db);
        await service.ToggleFavoriteAsync(TestUserId, 1);

        Assert.True(await service.IsFavoriteAsync(TestUserId, 1));

        var cut = _ctx.RenderComponent<Home>();

        cut.Find("#recipe-search").Input("Pancakes");

        Assert.Contains("Pancakes", cut.Markup);
        Assert.DoesNotContain("No recipes match your search", cut.Markup);
    }

    [Fact]
    public async Task Search_FindsRecipe_AfterFavoritingThroughTheDetailPage()
    {
        // Mirror the user's flow inside one circuit: open the detail
        // page, click Favorite, then go back to the home page.
        var detail = _ctx.RenderComponent<RecipeDetail>(p => p.Add(c => c.Id, 1));

        var favoriteButton = detail.FindAll("button")
            .First(b => b.TextContent.Contains("Favorite", StringComparison.OrdinalIgnoreCase));

        await favoriteButton.ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        Assert.Contains("Favorited", detail.Markup);

        var cut = _ctx.RenderComponent<Home>();

        cut.Find("#recipe-search").Input("Pancakes");

        Assert.Contains("Pancakes", cut.Markup);
        Assert.DoesNotContain("No recipes match your search", cut.Markup);
    }

    [Fact]
    public void Home_ShowsFavoriteHeartForAuthenticatedUser()
    {
        var cut = _ctx.RenderComponent<Home>();

        Assert.NotEmpty(cut.FindAll("button.favorite-btn"));
    }

    [Fact]
    public async Task Home_HeartReflectsFavoriteState()
    {
        await new RecipeService(_db).ToggleFavoriteAsync(TestUserId, 1);

        var cut = _ctx.RenderComponent<Home>();

        cut.Find("#recipe-search").Input("Pancakes");

        var heart = cut.Find("button.favorite-btn");

        Assert.Contains("is-favorite", heart.ClassName ?? string.Empty);
    }

    [Fact]
    public async Task Favoriting_FromHome_KeepsRecipeInSearchResults()
    {
        var cut = _ctx.RenderComponent<Home>();

        cut.Find("#recipe-search").Input("Pancakes");

        Assert.Contains("Pancakes", cut.Markup);
        Assert.Single(cut.FindAll("button.favorite-btn"));

        // Favorite the recipe while the search is active.
        await cut.Find("button.favorite-btn").ClickAsync(
            new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // The recipe must still be listed, now shown as favorited.
        Assert.Contains("Pancakes", cut.Markup);
        Assert.DoesNotContain("No recipes match your search", cut.Markup);
        Assert.Contains("is-favorite", cut.Find("button.favorite-btn").ClassName ?? string.Empty);
        Assert.True(await new RecipeService(_db).IsFavoriteAsync(TestUserId, 1));

        // Unfavoriting must not remove it either.
        await cut.Find("button.favorite-btn").ClickAsync(
            new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        Assert.Contains("Pancakes", cut.Markup);
        Assert.DoesNotContain("is-favorite", cut.Find("button.favorite-btn").ClassName ?? string.Empty);
        Assert.False(await new RecipeService(_db).IsFavoriteAsync(TestUserId, 1));
    }

    [Fact]
    public async Task Favoriting_DoesNotChangeTheSearchText()
    {
        var cut = _ctx.RenderComponent<Home>();

        cut.Find("#recipe-search").Input("Pancakes");

        await cut.Find("button.favorite-btn").ClickAsync(
            new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        Assert.Equal("Pancakes", cut.Find("#recipe-search").GetAttribute("value"));
        Assert.Single(cut.FindAll("article.recipe-card"));
    }

    [Fact]
    public async Task Favorites_AreNotShownToAnonymousVisitors()
    {
        using var anonymous = new TestContext();

        anonymous.Services.AddSingleton(_db);
        anonymous.Services.AddSingleton(new RecipeService(_db));
        anonymous.Services.AddSingleton<AuthenticationStateProvider>(new AnonymousAuthStateProvider());
        anonymous.Services.AddAuthorizationCore();
        anonymous.Services.AddCascadingAuthenticationState();

        await new RecipeService(_db).ToggleFavoriteAsync(TestUserId, 1);

        var cut = anonymous.RenderComponent<Home>();

        cut.Find("#recipe-search").Input("Pancakes");

        Assert.Contains("Pancakes", cut.Markup);
        Assert.Empty(cut.FindAll("button.favorite-btn"));
    }

    public void Dispose()
    {
        _ctx.Dispose();
        _db.Dispose();
        _connection.Dispose();

        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, true);
        }
    }
}
