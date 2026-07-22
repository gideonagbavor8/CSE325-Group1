/*
 * ===========================================================
 * ChefConnect - CSE 325 Group Project
 *
 * File: User.cs
 * Contributors:
 *   - Godfred Sefa Aboagye
 *   - Kamohelo Godfrey Mejaele
 *
 * Purpose:
 * This file was developed as part of our assigned contribution
 * for the ChefConnect project. It defines the User entity for
 * the database schema using Entity Framework Core.
 *
 * Related Database Tables:
 *   - Users
 *   - Recipes (One-to-Many Relationship)
 *
 * Last Updated:
 * July 2026
 * ===========================================================
 */


using System.ComponentModel.DataAnnotations;

namespace ChefConnect.Models
{
    /// <summary>
    /// Represents a registered user of the ChefConnect application.
    /// A user can create many recipes.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Primary Key.
        /// Entity Framework automatically treats "Id" as the primary key.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User's first name.
        /// Required and limited to 50 characters.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// User's last name.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Username used for logging into the application.
        /// Must be unique (enforced later in EF configuration/database).
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// User's email address.
        /// </summary>
        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Stores the hashed password.
        /// Never store plain text passwords.
        /// </summary>
        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Optional profile picture.
        /// </summary>
        public string? ProfileImage { get; set; }

        /// <summary>
        /// Date and time when the account was created.
        /// Defaults to the current date and time.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Navigation property.
        /// One User can create many Recipes.
        /// </summary>
        public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    }
}