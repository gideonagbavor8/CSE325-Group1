using ChefConnect.Data;
using ChefConnect.Models;
using Microsoft.EntityFrameworkCore;

public class RecipeService
{
    private readonly ApplicationDbContext _context;

    public RecipeService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Recipe>> GetAllRecipesAsync()
    {
        return await _context.Recipes.Include(r => r.Category).ToListAsync();
    }

    public async Task<Recipe?> GetRecipeByIdAsync(int id)
    {
        return await _context.Recipes.Include(r => r.Category).FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task AddRecipeAsync(Recipe recipe)
    {
        if (string.IsNullOrEmpty(recipe.UserId))
        {
            var defaultUser = await _context.Users.FirstOrDefaultAsync();
            if (defaultUser != null)
            {
                recipe.UserId = defaultUser.Id;
            }
        }

        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRecipeAsync(Recipe recipe)
    {
        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteRecipeAsync(int id)
    {
        var recipe = await _context.Recipes.FindAsync(id);
        if (recipe != null)
        {
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
        }
    }

    ////////////////////////////////////////
    // User recipes
    ////////////////////////////////////////
    public async Task<ApplicationUser?> GetUserByIdAsync(string userId)
    {
        return await _context.Users.FindAsync(userId);
    }

    public async Task<List<Recipe>> GetRecipesByUserAsync(string userId)
    {
        return await _context.Recipes
            .Include(r => r.Category)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    ////////////////////////////////////////
    // Favorites
    ////////////////////////////////////////
    public async Task<List<Recipe>> GetFavoriteRecipesAsync(string userId)
    {
        return await _context.Favorites
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .Include(f => f.Recipe!)
            .ThenInclude(r => r!.Category)
            .Select(f => f.Recipe!)
            .ToListAsync();
    }

    public async Task<HashSet<int>> GetFavoriteRecipeIdsAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return new HashSet<int>();
        }

        var ids = await _context.Favorites
            .Where(f => f.UserId == userId)
            .Select(f => f.RecipeId)
            .ToListAsync();

        return ids.ToHashSet();
    }

    public async Task<bool> IsFavoriteAsync(string userId, int recipeId)
    {
        return await _context.Favorites.AnyAsync(f => f.UserId == userId && f.RecipeId == recipeId);
    }

    public async Task AddFavoriteAsync(string userId, int recipeId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return;
        }

        var exists = await IsFavoriteAsync(userId, recipeId);
        if (!exists)
        {
            _context.Favorites.Add(new Favorite { UserId = userId, RecipeId = recipeId });
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveFavoriteAsync(string userId, int recipeId)
    {
        var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);
        if (favorite != null)
        {
            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ToggleFavoriteAsync(string userId, int recipeId)
    {
        var favorite = await _context.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.RecipeId == recipeId);

        if (favorite != null)
        {
            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            return false;
        }
        else
        {
            _context.Favorites.Add(new Favorite { UserId = userId, RecipeId = recipeId });
            await _context.SaveChangesAsync();

            return true;
        }
    }
}