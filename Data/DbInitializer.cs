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


using ChefConnect.Models;

namespace ChefConnect.Data
{
    /// <summary>
    /// Seeds the database with sample data.
    /// This ensures the application has initial data
    /// for testing and demonstration.
    /// </summary>
    public static class DbInitializer
    {
        public static void Initialize(ChefConnectContext context)
        {
            // Create the database if it doesn't exist.
            context.Database.EnsureCreated();

            // If data already exists, stop here.
            if (context.Users.Any())
            {
                return;
            }

            // -----------------------------
            // Seed Users
            // -----------------------------
            var users = new User[]
            {
                new User
                {
                    FirstName = "Godfred",
                    LastName = "Aboagye",
                    Username = "gsefa",
                    Email = "gsefa@example.com",
                    PasswordHash = "DemoPasswordHash"
                },

                new User
                {
                    FirstName = "Kamohelo",
                    LastName = "Mejaele",
                    Username = "kmejaele",
                    Email = "kamohelo@example.com",
                    PasswordHash = "DemoPasswordHash"
                }
            };

            context.Users.AddRange(users);
            context.SaveChanges();

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
                    UserId = users[1].Id,
                    CategoryId = categories[2].Id
                }
            };

            context.Recipes.AddRange(recipes);
            context.SaveChanges();
        }
    }
}