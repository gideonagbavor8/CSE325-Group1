using Bunit;
using ChefConnect.Components.Pages.HomeComponents;
using ChefConnect.Components.Pages.Recipes;
using ChefConnect.Data;
using ChefConnect.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Security.Claims;
using Xunit;

namespace ChefConnect.Tests;

/// <summary>
/// Exercises the create/edit recipe forms and the display
/// components through bUnit so the image behaviour is verified
/// end to end (file chosen -> validated -> stored -> displayed).
/// </summary>
public class RecipeImageFormTests : IDisposable
{
    private const string TestUserId = "test-user-1";

    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _db;
    private readonly string _root;
    private readonly FakeEnvironment _env;
    private readonly TestContext _ctx = new();

    public RecipeImageFormTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new ApplicationDbContext(options);
        _db.Database.EnsureCreated();

        _db.Categories.Add(new Category { Id = 1, Name = "Dinner" });
        _db.Users.Add(new ApplicationUser
        {
            Id = TestUserId,
            UserName = "tester@example.com",
            NormalizedUserName = "TESTER@EXAMPLE.COM",
            Email = "tester@example.com",
            NormalizedEmail = "TESTER@EXAMPLE.COM",
            SecurityStamp = Guid.NewGuid().ToString()
        });
        _db.SaveChanges();

        _root = Path.Combine(Path.GetTempPath(), "chefconnect-ui-tests", Guid.NewGuid().ToString("N"));
        _env = new FakeEnvironment(_root);

        _ctx.Services.AddSingleton(new RecipeService(_db));
        _ctx.Services.AddSingleton<IWebHostEnvironment>(_env);
        _ctx.Services.AddSingleton<AuthenticationStateProvider>(new FakeAuthStateProvider(TestUserId));
        _ctx.Services.AddAuthorizationCore();
        _ctx.Services.AddCascadingAuthenticationState();
    }

    // =====================================================
    // Create
    // =====================================================

    [Fact]
    public async Task Create_WithUploadedImageOnly_StoresLocalPathAndWritesFile()
    {
        var cut = _ctx.RenderComponent<RecipeCreate>();

        FillRequiredFields(cut);
        UploadFile(cut, "my holiday photo.JPG", "image/jpeg", new byte[] { 1, 2, 3 });

        await cut.Find("form").SubmitAsync();

        var recipe = Assert.Single(await _db.Recipes.ToListAsync());

        Assert.NotNull(recipe.ImageUrl);
        Assert.StartsWith("/uploads/recipes/", recipe.ImageUrl);
        Assert.EndsWith(".jpg", recipe.ImageUrl);
        Assert.DoesNotContain("holiday", recipe.ImageUrl);

        var physicalPath = Path.Combine(
            _root, "wwwroot", "uploads", "recipes", Path.GetFileName(recipe.ImageUrl!));

        Assert.True(File.Exists(physicalPath), "the uploaded file was not written to disk");
        Assert.Equal(new byte[] { 1, 2, 3 }, await File.ReadAllBytesAsync(physicalPath));
    }

    [Fact]
    public async Task Create_WithImageUrlOnly_StoresTheUrl()
    {
        var cut = _ctx.RenderComponent<RecipeCreate>();

        FillRequiredFields(cut);
        cut.Find("#recipe-image-url").Change("https://example.com/recipe.jpg");

        await cut.Find("form").SubmitAsync();

        var recipe = Assert.Single(await _db.Recipes.ToListAsync());

        Assert.Equal("https://example.com/recipe.jpg", recipe.ImageUrl);
    }

    [Fact]
    public async Task Create_WithBothImages_UploadedImageWins()
    {
        var cut = _ctx.RenderComponent<RecipeCreate>();

        FillRequiredFields(cut);
        cut.Find("#recipe-image-url").Change("https://example.com/should-be-ignored.jpg");
        UploadFile(cut, "photo.png", "image/png", new byte[] { 9, 9 });

        await cut.Find("form").SubmitAsync();

        var recipe = Assert.Single(await _db.Recipes.ToListAsync());

        Assert.StartsWith("/uploads/recipes/", recipe.ImageUrl);
        Assert.EndsWith(".png", recipe.ImageUrl);
    }

    [Fact]
    public async Task Create_WithNoImage_SavesRecipeWithoutImage()
    {
        var cut = _ctx.RenderComponent<RecipeCreate>();

        FillRequiredFields(cut);

        await cut.Find("form").SubmitAsync();

        var recipe = Assert.Single(await _db.Recipes.ToListAsync());

        Assert.Null(recipe.ImageUrl);
    }

    [Fact]
    public async Task Create_WithInvalidFileType_ShowsMessageAndSavesNothing()
    {
        var cut = _ctx.RenderComponent<RecipeCreate>();

        FillRequiredFields(cut);
        UploadFile(cut, "malware.exe", "application/octet-stream", new byte[] { 1 });

        Assert.Contains(
            "Only JPG, JPEG, PNG, and WEBP images can be uploaded.",
            cut.Markup);

        await cut.Find("form").SubmitAsync();

        // The rejected file is discarded, so the recipe is saved
        // without an image rather than with the rejected file.
        var recipe = Assert.Single(await _db.Recipes.ToListAsync());

        Assert.Null(recipe.ImageUrl);
        Assert.Empty(UploadedFiles());
    }

    [Fact]
    public async Task Create_WithOversizedFile_ShowsMessageAndSavesNothing()
    {
        var cut = _ctx.RenderComponent<RecipeCreate>();

        FillRequiredFields(cut);

        // 6 MB, over the 5 MB limit.
        UploadFile(cut, "huge.jpg", "image/jpeg", new byte[6 * 1024 * 1024]);

        Assert.Contains("The image must be 5 MB or smaller.", cut.Markup);

        await cut.Find("form").SubmitAsync();

        var recipe = Assert.Single(await _db.Recipes.ToListAsync());

        Assert.Null(recipe.ImageUrl);
        Assert.Empty(UploadedFiles());
    }

    [Fact]
    public async Task Create_WithInvalidImageUrl_ShowsMessageAndDoesNotSave()
    {
        var cut = _ctx.RenderComponent<RecipeCreate>();

        FillRequiredFields(cut);
        cut.Find("#recipe-image-url").Change("not-a-url");

        await cut.Find("form").SubmitAsync();

        Assert.Contains("Enter a valid image URL", cut.Markup);
        Assert.Empty(await _db.Recipes.ToListAsync());
    }

    [Fact]
    public void Create_ShowsBothImageOptions()
    {
        var cut = _ctx.RenderComponent<RecipeCreate>();

        Assert.Contains("Upload from your computer", cut.Markup);
        Assert.Contains("Image URL", cut.Markup);
        Assert.Contains("OR", cut.Markup);
        Assert.Equal(".jpg,.jpeg,.png,.webp", cut.Find("#recipe-image-file").GetAttribute("accept"));
    }

    // =====================================================
    // Edit
    // =====================================================

    [Fact]
    public async Task Edit_WithoutChangingImage_KeepsExistingExternalUrl()
    {
        var id = await SeedRecipeAsync("https://example.com/original.jpg");

        var cut = _ctx.RenderComponent<RecipeEdit>(p => p.Add(c => c.Id, id));

        await cut.Find("form").SubmitAsync();

        var recipe = await _db.Recipes.AsNoTracking().FirstAsync(r => r.Id == id);

        Assert.Equal("https://example.com/original.jpg", recipe.ImageUrl);
    }

    [Fact]
    public async Task Edit_WithoutChangingImage_KeepsExistingUploadedImage()
    {
        var id = await SeedRecipeAsync("/uploads/recipes/abc123.jpg");

        var cut = _ctx.RenderComponent<RecipeEdit>(p => p.Add(c => c.Id, id));

        // An uploaded path is never placed in the Image URL field.
        Assert.Equal(string.Empty, cut.Find("#recipe-image-url").GetAttribute("value") ?? string.Empty);

        await cut.Find("form").SubmitAsync();

        var recipe = await _db.Recipes.AsNoTracking().FirstAsync(r => r.Id == id);

        Assert.Equal("/uploads/recipes/abc123.jpg", recipe.ImageUrl);
    }

    [Fact]
    public async Task Edit_UploadReplacesExistingImageUrl()
    {
        var id = await SeedRecipeAsync("https://example.com/original.jpg");

        var cut = _ctx.RenderComponent<RecipeEdit>(p => p.Add(c => c.Id, id));

        UploadFile(cut, "replacement.webp", "image/webp", new byte[] { 7 });

        await cut.Find("form").SubmitAsync();

        var recipe = await _db.Recipes.AsNoTracking().FirstAsync(r => r.Id == id);

        Assert.StartsWith("/uploads/recipes/", recipe.ImageUrl);
        Assert.EndsWith(".webp", recipe.ImageUrl);
    }

    [Fact]
    public async Task Edit_ClearingImageUrlRemovesTheImage()
    {
        var id = await SeedRecipeAsync("https://example.com/original.jpg");

        var cut = _ctx.RenderComponent<RecipeEdit>(p => p.Add(c => c.Id, id));

        cut.Find("#recipe-image-url").Change(string.Empty);

        await cut.Find("form").SubmitAsync();

        var recipe = await _db.Recipes.AsNoTracking().FirstAsync(r => r.Id == id);

        Assert.Null(recipe.ImageUrl);
    }

    // =====================================================
    // Display
    // =====================================================

    [Theory]
    [InlineData("https://example.com/photo.jpg", "https://example.com/photo.jpg")]
    [InlineData("/uploads/recipes/abc123.png", "/uploads/recipes/abc123.png")]
    public void RecipeCard_DisplaysBothImageKinds(string stored, string expectedSource)
    {
        var cut = _ctx.RenderComponent<RecipeCard>(p => p
            .Add(c => c.Recipe, new Recipe { Id = 1, Name = "Soup", ImageUrl = stored }));

        Assert.Equal(expectedSource, cut.Find("img.recipe-image").GetAttribute("src"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("javascript:alert(1)")]
    public void RecipeCard_FallsBackToPlaceholder(string? stored)
    {
        var cut = _ctx.RenderComponent<RecipeCard>(p => p
            .Add(c => c.Recipe, new Recipe { Id = 1, Name = "Soup", ImageUrl = stored }));

        Assert.Empty(cut.FindAll("img.recipe-image"));
        Assert.Contains("No photo", cut.Markup);
    }

    [Theory]
    [InlineData("https://example.com/photo.jpg")]
    [InlineData("/uploads/recipes/abc123.png")]
    public async Task RecipeDetail_DisplaysBothImageKinds(string stored)
    {
        var id = await SeedRecipeAsync(stored);

        var cut = _ctx.RenderComponent<RecipeDetail>(p => p.Add(c => c.Id, id));

        Assert.Equal(stored, cut.Find("img.recipe-image-detail").GetAttribute("src"));
    }

    [Fact]
    public async Task RecipeDetail_WithoutImage_ShowsPlaceholder()
    {
        var id = await SeedRecipeAsync(null);

        var cut = _ctx.RenderComponent<RecipeDetail>(p => p.Add(c => c.Id, id));

        Assert.Empty(cut.FindAll("img.recipe-image-detail"));
        Assert.Single(cut.FindAll(".recipe-detail-placeholder"));
    }

    // =====================================================
    // Helpers
    // =====================================================

    private static void FillRequiredFields(IRenderedFragment cut)
    {
        cut.Find("#recipe-name").Change("Test Recipe");
        cut.Find("#recipe-description").Change("A description.");
        cut.Find("#recipe-category").Change("1");
        cut.Find("#recipe-ingredients").Change("Salt");
        cut.Find("#recipe-instructions").Change("Cook it.");
        cut.Find("#recipe-preparation").Change("10");
        cut.Find("#recipe-cooking").Change("20");
        cut.Find("#recipe-servings").Change("4");
    }

    private static void UploadFile(IRenderedFragment cut, string name, string contentType, byte[] content)
    {
        var file = InputFileContent.CreateFromBinary(content, name, contentType: contentType);

        cut.FindComponent<Microsoft.AspNetCore.Components.Forms.InputFile>()
            .UploadFiles(file);
    }

    /// <summary>
    /// Files written to the uploads folder. The folder only exists
    /// once an upload has been accepted, so a missing folder simply
    /// means nothing was written.
    /// </summary>
    private string[] UploadedFiles()
    {
        var directory = Path.Combine(_root, "wwwroot", "uploads", "recipes");

        return Directory.Exists(directory)
            ? Directory.GetFiles(directory)
            : Array.Empty<string>();
    }

    private async Task<int> SeedRecipeAsync(string? imageUrl)
    {
        var recipe = new Recipe
        {
            Name = "Seeded",
            Description = "d",
            Ingredients = "i",
            Instructions = "n",
            PreparationTime = 5,
            CookingTime = 5,
            Servings = 2,
            CategoryId = 1,
            UserId = TestUserId,
            ImageUrl = imageUrl
        };

        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync();

        _db.ChangeTracker.Clear();

        return recipe.Id;
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

internal sealed class FakeAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthenticationState _state;

    public FakeAuthStateProvider(string userId)
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, "tester")
            },
            "TestAuth");

        _state = new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
        => Task.FromResult(_state);
}

internal sealed class FakeEnvironment : IWebHostEnvironment
{
    public FakeEnvironment(string root)
    {
        ContentRootPath = root;
        WebRootPath = Path.Combine(root, "wwwroot");
        Directory.CreateDirectory(WebRootPath);
        ContentRootFileProvider = new PhysicalFileProvider(root);
        WebRootFileProvider = new PhysicalFileProvider(WebRootPath);
    }

    public string WebRootPath { get; set; }
    public IFileProvider WebRootFileProvider { get; set; }
    public string ApplicationName { get; set; } = "UiTests";
    public IFileProvider ContentRootFileProvider { get; set; }
    public string ContentRootPath { get; set; }
    public string EnvironmentName { get; set; } = "Development";
}
