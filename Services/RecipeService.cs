/*
 * ===========================================================
 * ChefConnect - CSE 325 Group Project
 *
 * File: RecipeService.cs
 *
 * Contributors:
 *   - Godfred Sefa Aboagye
 *   - Kamohelo Godfrey Mejaele
 *
 * Purpose:
 * Provides database operations for recipes, categories,
 * user recipes, and favorite recipes.
 *
 * Responsibilities:
 *   • Retrieve all recipes
 *   • Retrieve individual recipes
 *   • Retrieve categories
 *   • Create recipes
 *   • Update recipes
 *   • Delete recipes
 *   • Retrieve user information
 *   • Retrieve recipes belonging to a user
 *   • Manage favorite recipes
 * ===========================================================
 */

using ChefConnect.Data;
using ChefConnect.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Provides database operations for ChefConnect recipes,
/// categories, users, and favorites.
/// </summary>
public class RecipeService
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Creates a new RecipeService using the application's
    /// Entity Framework database context.
    /// </summary>
    /// <param name="context">
    /// The Entity Framework Core database context.
    /// </param>
    public RecipeService(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // Recipe Operations
    // =========================================================

    /// <summary>
    /// Retrieves all recipes from the database.
    /// Recipes are returned with their associated categories.
    /// </summary>
    public async Task<List<Recipe>> GetAllRecipesAsync()
    {
        return await _context.Recipes
            .Include(r => r.Category)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a single recipe by its ID.
    /// The associated category is also loaded.
    /// </summary>
    /// <param name="id">The recipe ID.</param>
    /// <returns>
    /// The recipe if found; otherwise null.
    /// </returns>
    public async Task<Recipe?> GetRecipeByIdAsync(int id)
    {
        return await _context.Recipes
            .Include(r => r.Category)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <summary>
    /// Retrieves all available recipe categories.
    /// </summary>
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    /// <summary>
    /// Adds a new recipe to the database.
    ///
    /// A valid UserId must already be assigned by the
    /// authenticated user before this method is called.
    /// </summary>
    /// <param name="recipe">The recipe to add.</param>
    public async Task AddRecipeAsync(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        // A recipe must belong to an authenticated user.
        if (string.IsNullOrWhiteSpace(recipe.UserId))
        {
            throw new InvalidOperationException(
                "A recipe must have an authenticated user.");
        }

        // Validate the category before saving.
        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == recipe.CategoryId);

        if (!categoryExists)
        {
            throw new InvalidOperationException(
                "The selected recipe category does not exist.");
        }

        // Add the recipe to Entity Framework's change tracker.
        _context.Recipes.Add(recipe);

        // Save the new recipe to the database.
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Updates an existing recipe.
    /// </summary>
    /// <param name="recipe">The updated recipe.</param>
    public async Task UpdateRecipeAsync(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        if (string.IsNullOrWhiteSpace(recipe.UserId))
        {
            throw new InvalidOperationException(
                "A recipe must have an owner.");
        }

        var existingRecipe = await _context.Recipes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == recipe.Id);

        if (existingRecipe == null)
        {
            throw new InvalidOperationException(
                "The recipe could not be found.");
        }

        var categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == recipe.CategoryId);

        if (!categoryExists)
        {
            throw new InvalidOperationException(
                "The selected recipe category does not exist.");
        }

        // Update the recipe.
        _context.Recipes.Update(recipe);

        // Save changes.
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a recipe using its ID.
    /// </summary>
    /// <param name="id">The recipe ID.</param>
    public async Task DeleteRecipeAsync(int id)
    {
        var recipe = await _context.Recipes
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe == null)
        {
            return;
        }

        _context.Recipes.Remove(recipe);

        await _context.SaveChangesAsync();
    }

    // =========================================================
    // User Recipe Operations
    // =========================================================

    /// <summary>
    /// Retrieves a user by their ASP.NET Core Identity ID.
    /// </summary>
    /// <param name="userId">The Identity user ID.</param>
    /// <returns>
    /// The user if found; otherwise null.
    /// </returns>
    public async Task<ApplicationUser?> GetUserByIdAsync(
        string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        return await _context.Users
            .FindAsync(userId);
    }

    /// <summary>
    /// Retrieves all recipes created by a specific user.
    /// </summary>
    /// <param name="userId">The user's Identity ID.</param>
    public async Task<List<Recipe>> GetRecipesByUserAsync(
        string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new List<Recipe>();
        }

        return await _context.Recipes
            .Include(r => r.Category)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    // =========================================================
    // Favorite Operations
    // =========================================================

    /// <summary>
    /// Retrieves all recipes favorited by a specific user.
    /// </summary>
    /// <param name="userId">The user's Identity ID.</param>
    public async Task<List<Recipe>> GetFavoriteRecipesAsync(
        string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new List<Recipe>();
        }

        return await _context.Favorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Include(f => f.Recipe!)
            .ThenInclude(r => r!.Category)
            .Select(f => f.Recipe!)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves the IDs of all recipes favorited by a user.
    ///
    /// HashSet is used because it provides efficient lookup
    /// when checking whether a recipe has been favorited.
    /// </summary>
    /// <param name="userId">The user's Identity ID.</param>
    public async Task<HashSet<int>> GetFavoriteRecipeIdsAsync(
        string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return new HashSet<int>();
        }

        var ids = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Select(f => f.RecipeId)
            .ToListAsync();

        return ids.ToHashSet();
    }

    /// <summary>
    /// Determines whether a user has favorited a recipe.
    /// </summary>
    /// <param name="userId">The user's Identity ID.</param>
    /// <param name="recipeId">The recipe ID.</param>
    public async Task<bool> IsFavoriteAsync(
        string userId,
        int recipeId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        return await _context.Favorites
            .AnyAsync(f =>
                f.UserId == userId &&
                f.RecipeId == recipeId);
    }

    /// <summary>
    /// Adds a recipe to the user's favorites.
    ///
    /// If the recipe is already favorited, no duplicate
    /// favorite is created.
    /// </summary>
    public async Task AddFavoriteAsync(
        string userId,
        int recipeId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        // Prevent duplicate favorites.
        var exists = await IsFavoriteAsync(
            userId,
            recipeId);

        if (exists)
        {
            return;
        }

        // Make sure the recipe exists.
        var recipeExists = await _context.Recipes
            .AnyAsync(r => r.Id == recipeId);

        if (!recipeExists)
        {
            return;
        }

        var favorite = new Favorite
        {
            UserId = userId,
            RecipeId = recipeId
        };

        _context.Favorites.Add(favorite);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Removes a recipe from the user's favorites.
    /// </summary>
    public async Task RemoveFavoriteAsync(
        string userId,
        int recipeId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f =>
                f.UserId == userId &&
                f.RecipeId == recipeId);

        if (favorite == null)
        {
            return;
        }

        _context.Favorites.Remove(favorite);

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Adds or removes a recipe from a user's favorites.
    ///
    /// Returns true when the recipe is now favorited.
    /// Returns false when the recipe has been removed
    /// from favorites.
    /// </summary>
    public async Task<bool> ToggleFavoriteAsync(
        string userId,
        int recipeId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return false;
        }

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f =>
                f.UserId == userId &&
                f.RecipeId == recipeId);

        // If a favorite exists, remove it.
        if (favorite != null)
        {
            _context.Favorites.Remove(favorite);

            await _context.SaveChangesAsync();

            return false;
        }

        // Make sure the recipe exists before creating
        // a new favorite record.
        var recipeExists = await _context.Recipes
            .AnyAsync(r => r.Id == recipeId);

        if (!recipeExists)
        {
            return false;
        }

        // Otherwise create a new favorite.
        _context.Favorites.Add(
            new Favorite
            {
                UserId = userId,
                RecipeId = recipeId
            });

        await _context.SaveChangesAsync();

        return true;
    }
}