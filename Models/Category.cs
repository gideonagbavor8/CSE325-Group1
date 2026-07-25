/*
 * ===========================================================
 * ChefConnect - CSE 325 Group Project
 *
 * File: Category.cs
 * Contributors:
 *   - Godfred Sefa Aboagye
 *   - Kamohelo Godfrey Mejaele
 *
 * Purpose:
 * Defines recipe categories and establishes the one-to-many
 * relationship between Categories and Recipes.
 * ===========================================================
 */


using System.ComponentModel.DataAnnotations;

namespace ChefConnect.Models
{
    /// <summary>
    /// Represents a recipe category.
    /// Examples include Breakfast, Lunch, Dinner, Dessert, etc.
    /// </summary>
    public class Category
    {
        /// <summary>
        /// Primary Key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Name of the category.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the category.
        /// </summary>
        [StringLength(300)]
        public string? Description { get; set; }

        /// <summary>
        /// Navigation property.
        /// One category can contain many recipes.
        /// </summary>
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}