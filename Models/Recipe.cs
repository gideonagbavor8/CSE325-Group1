/*
 * ===========================================================
 * ChefConnect - CSE 325 Group Project
 *
 * File: Recipe.cs
 * Contributors:
 *   - Godfred Sefa Aboagye
 *   - Kamohelo Godfrey Mejaele
 *
 * Purpose:
 * Defines the Recipe entity and its relationships with
 * Users and Categories using Entity Framework Core.
 * ===========================================================
 */


using System.ComponentModel.DataAnnotations;

namespace ChefConnect.Models
{
    /// <summary>
    /// Represents a recipe created by a user.
    /// Each recipe belongs to one category and one user.
    /// </summary>
    public class Recipe
    {
        /// <summary>
        /// Primary Key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the recipe.
        /// </summary>
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Short description of the recipe.
        /// </summary>
        [Required]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// List of ingredients needed.
        /// Stored as text for simplicity.
        /// </summary>
        [Required]
        public string Ingredients { get; set; } = string.Empty;

        /// <summary>
        /// Step-by-step cooking instructions.
        /// </summary>
        [Required]
        public string Instructions { get; set; } = string.Empty;

        /// <summary>
        /// Preparation time in minutes.
        /// </summary>
        [Range(1, 1000)]
        public int PreparationTime { get; set; }

        /// <summary>
        /// Cooking time in minutes.
        /// </summary>
        [Range(1, 1000)]
        public int CookingTime { get; set; }

        /// <summary>
        /// Number of people the recipe serves.
        /// </summary>
        [Range(1, 100)]
        public int Servings { get; set; }

        /// <summary>
        /// Optional image URL or relative image path.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Date the recipe was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // ============================
        // Foreign Keys
        // ============================

        /// <summary>
        /// Foreign key linking the recipe to the user who created it.
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Navigation property to the recipe's author.
        /// </summary>
        public User? User { get; set; }

        /// <summary>
        /// Foreign key linking the recipe to a category.
        /// </summary>
        public int CategoryId { get; set; }

        /// <summary>
        /// Navigation property to the recipe's category.
        /// </summary>
        public Category? Category { get; set; }
    }
}