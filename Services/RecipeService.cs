using ChefConnect.Data;
using ChefConnect.Models;
using Microsoft.EntityFrameworkCore;

public class RecipeService
{
    private readonly ChefConnectContext _context;

    public RecipeService(ChefConnectContext context)
    {
        _context = context;
    }

    public async Task<List<Recipe>> GetAllRecipesAsync()

    {
        return await _context.Recipes.ToListAsync();
    }
    public async Task<Recipe?> GetRecipeByIdAsync(int id)
    {
        return await _context.Recipes.FindAsync(id);
    }
}