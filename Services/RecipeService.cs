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
}