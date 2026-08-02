/*
 * ===========================================================
 * ChefConnect - CSE 325 Group Project
 *
 * File: DbInitializer.cs
 * Contributors:
 *   - Godfred Sefa Aboagye
 *   - Kamohelo Godfrey Mejaele
 *
 * Purpose:
 * Seeds the database with sample users, recipe categories,
 * and recipes for testing and demonstration purposes.
 * ===========================================================
 */

using ChefConnect.Data;
using ChefConnect.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ChefConnect.Data
{
    /// <summary>
    /// Seeds the database with sample data.
    /// This ensures the application has initial data
    /// for testing and demonstration.
    /// </summary>
    public static class DbInitializer
    {
        // Temp password for the seeded accounts (Identity's default complexity rules).
        //  Users set their own password through /Register endpoint.
        private const string TempPass = "Passw0rd!";

        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Using Migrate() instead of EnsureCreated() so that schema changes
            // are tracked through EF Core migrations. EnsureCreated() only creates
            // the database if it doesn't already exist and cannot apply future
            // schema changes. Migrate() allows the team to evolve the database
            // (e.g., adding new fields or tables) without losing existing data.
            context.Database.Migrate();

            // If data already exists, stop here.
            if (context.Users.Any())
            {
                return;
            }

            // -----------------------------
            // Seed Users
            // -----------------------------
            // Created with UserManager (not inserted directly) so passwords are
            //  properly hashed and the normalized username/email fields Identity
            //  relies on for login are populated correctly.
            var users = new ApplicationUser[]
            {
                new ApplicationUser
                {
                    FirstName = "Godfred",
                    LastName = "Aboagye",
                    UserName = "gsefa@example.com",
                    Email = "gsefa@example.com",
                },

                new ApplicationUser
                {
                    FirstName = "Kamohelo",
                    LastName = "Mejaele",
                    UserName = "kamohelo@example.com",
                    Email = "kamohelo@example.com",
                }
            };

            foreach (var user in users)
            {
                var result = await userManager.CreateAsync(user, TempPass);
                if (!result.Succeeded)
                {
                    throw new InvalidOperationException($"Failed to seed user {user.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // -----------------------------
            // Seed Categories
            // -----------------------------
            var categories = new Category[]
            {
                new Category { Name = "Breakfast" },
                new Category { Name = "Lunch" },
                new Category { Name = "Dinner" },
                new Category { Name = "Dessert" }
            };

            context.Categories.AddRange(categories);
            context.SaveChanges();

            // -----------------------------
            // Seed Recipes
            // -----------------------------
            var recipes = new Recipe[]
            {
                new Recipe
                {
                    Name = "Pancakes",
                    Description = "Classic homemade pancakes.",
                    Ingredients = "Flour, Eggs, Milk, Sugar",
                    Instructions = "Mix ingredients and cook on a pan.",
                    PreparationTime = 10,
                    CookingTime = 15,
                    Servings = 4,
                    ImageUrl = "https://images.unsplash.com/photo-1533089860892-a7c6f0a88666?auto=format&fit=crop&w=900&q=80",
                    UserId = users[0].Id,
                    CategoryId = categories[0].Id
                },

                new Recipe
                {
                    Name = "Jollof Rice",
                    Description = "Traditional West African Jollof Rice.",
                    Ingredients = "Rice, Tomato, Pepper, Onion",
                    Instructions = "Cook rice with tomato sauce.",
                    PreparationTime = 20,
                    CookingTime = 45,
                    Servings = 6,
                    ImageUrl = "https://images.unsplash.com/photo-1528735602780-2552fd46c7af?auto=format&fit=crop&w=900&q=80",
                    UserId = users[1].Id,
                    CategoryId = categories[2].Id
                }
            };

            context.Recipes.AddRange(recipes);
            context.SaveChanges();
        }
    }
}