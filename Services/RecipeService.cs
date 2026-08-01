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
        return await _context.Recipes.ToListAsync();
    }
    public async Task<Recipe?> GetRecipeByIdAsync(int id)
    {
        return await _context.Recipes.FindAsync(id);
    }
}