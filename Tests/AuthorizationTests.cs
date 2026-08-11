using Bunit;
using ChefConnect.Components.Pages.Recipes;
using ChefConnect.Data;
using ChefConnect.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;
using Xunit;

namespace ChefConnect.Tests;

public class AuthorizationTests : IDisposable
{
    private const string OwnerId = "owner-user";
    private const string OtherId = "other-user";

    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _db;
    private readonly RecipeService _service;
    private int _recipeId;

    public AuthorizationTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        _db.Categories.Add(new Category { Id = 1, Name = "Dinner" });
        foreach (var id in new[] { OwnerId, OtherId })
        {
            _db.Users.Add(new ApplicationUser
            {
                Id = id, UserName = id + "@e.com", NormalizedUserName = (id + "@E.COM").ToUpper(),
                SecurityStamp = Guid.NewGuid().ToString()
            });
        }
        var recipe = new Recipe
        {
            Name = "Pancakes", Description = "d", Ingredients = "i", Instructions = "n",
            PreparationTime = 5, CookingTime = 5, Servings = 2, CategoryId = 1, UserId = OwnerId
        };
        _db.Recipes.Add(recipe);
        _db.SaveChanges();
        _db.ChangeTracker.Clear();
        _recipeId = recipe.Id;
        _service = new RecipeService(_db);
    }

    // ---------- service layer: the part a hidden button cannot protect ----------

    [Fact]
    public async Task Delete_AnonymousUser_IsRefused()
    {
        Assert.False(await _service.DeleteRecipeAsync(_recipeId, null));
        Assert.False(await _service.DeleteRecipeAsync(_recipeId, ""));
        Assert.Equal(1, await _db.Recipes.CountAsync());
    }

    [Fact]
    public async Task Delete_DifferentUser_IsRefused()
    {
        Assert.False(await _service.DeleteRecipeAsync(_recipeId, OtherId));
        Assert.Equal(1, await _db.Recipes.CountAsync());
    }

    [Fact]
    public async Task Delete_Owner_Succeeds()
    {
        Assert.True(await _service.DeleteRecipeAsync(_recipeId, OwnerId));
        Assert.Equal(0, await _db.Recipes.CountAsync());
    }

    [Fact]
    public async Task Update_DifferentUserOrAnonymous_IsRefused()
    {
        var recipe = await _db.Recipes.AsNoTracking().FirstAsync();
        recipe.Name = "Hijacked";

        Assert.False(await _service.UpdateRecipeAsync(recipe, OtherId));
        Assert.False(await _service.UpdateRecipeAsync(recipe, null));

        _db.ChangeTracker.Clear();
        Assert.Equal("Pancakes", (await _db.Recipes.AsNoTracking().FirstAsync()).Name);
    }

    [Fact]
    public async Task Update_CannotReassignOwnerToSomeoneElse()
    {
        var recipe = await _db.Recipes.AsNoTracking().FirstAsync();
        recipe.UserId = OtherId;          // attempt to steal the recipe
        recipe.Name = "Renamed";

        Assert.True(await _service.UpdateRecipeAsync(recipe, OwnerId));

        _db.ChangeTracker.Clear();
        var stored = await _db.Recipes.AsNoTracking().FirstAsync();
        Assert.Equal(OwnerId, stored.UserId);
        Assert.Equal("Renamed", stored.Name);
    }

    // ---------- /recipes page ----------

    private (IRenderedComponent<RecipeList> Cut, TestContext Ctx) RenderListAs(string? signedInUserId)
    {
        var ctx = new TestContext();
        ctx.Services.AddSingleton(_db);
        ctx.Services.AddSingleton(new RecipeService(_db));
        ctx.Services.AddSingleton<AuthenticationStateProvider>(
            signedInUserId is null
                ? new AnonymousAuthStateProvider()
                : new FakeAuthStateProvider(signedInUserId));
        ctx.Services.AddAuthorizationCore();
        ctx.Services.AddCascadingAuthenticationState();
        _contexts.Add(ctx);
        return (ctx.RenderComponent<RecipeList>(), ctx);
    }

    private readonly List<TestContext> _contexts = new();

    [Fact]
    public void RecipesPage_SignedOut_RedirectsHomeAndShowsNothing()
    {
        var (cut, ctx) = RenderListAs(null);

        // Signed-out visitors are sent to the home page instead of
        // seeing anyone's recipes.
        var navigation = ctx.Services.GetRequiredService<NavigationManager>();

        Assert.EndsWith("/", navigation.Uri);
        Assert.DoesNotContain("Pancakes", cut.Markup);
        Assert.Empty(cut.FindAll("button.btn-outline-danger"));
    }

    [Fact]
    public void RecipesPage_ShowsOnlyTheSignedInUsersRecipes()
    {
        // The recipe belongs to OwnerId, so another user sees nothing
        // of it here, and has no way to delete it.
        var (cut, _) = RenderListAs(OtherId);

        Assert.DoesNotContain("Pancakes", cut.Markup);
        Assert.Empty(cut.FindAll("button.btn-outline-danger"));
        Assert.DoesNotContain("/recipes/edit/", cut.Markup);
    }

    [Fact]
    public async Task RecipesPage_Owner_CanDelete()
    {
        var (cut, _) = RenderListAs(OwnerId);

        await cut.Find("button.btn-outline-danger").ClickAsync(new MouseEventArgs());
        await cut.Find(".confirm-actions .btn-danger").ClickAsync(new MouseEventArgs());

        Assert.Equal(0, await _db.Recipes.CountAsync());
    }

    public void Dispose()
    {
        foreach (var c in _contexts) c.Dispose();
        _db.Dispose();
        _connection.Dispose();
    }
}
