/*
 * ===========================================================
 * ChefConnect - CSE 325 Group Project
 *
 * File: Favorites.cs
 * Contributors:
 *  - Robbie Yamashita
 *
 * Purpose:
 * Join entity representing a user's favorited recipe.
 * A user can favorite many recipes, and a recipe can be
 * favorited by many users (many to many with this table).
 * ===========================================================
 */

namespace ChefConnect.Models
{
    public class Favorite
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public ApplicationUser? User { get; set; }

        public int RecipeId { get; set; }

        public Recipe? Recipe { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
