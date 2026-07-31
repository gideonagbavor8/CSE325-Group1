using Microsoft.AspNetCore.Identity;
using ChefConnect.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? ProfileImage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}