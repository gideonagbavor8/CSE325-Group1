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
                    ImageUrl = "https://images.unsplash.com/photo-1528207776546-365bb710ee93?w=500&auto=format&fit=crop&q=60",
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
                    ImageUrl = "https://images.unsplash.com/photo-1666190092689-e3968aa0c32c?w=500&auto=format&fit=crop&q=60",
                    UserId = users[1].Id,
                    CategoryId = categories[2].Id
                },
                new Recipe
                {
                    Name = "Penne Pasta with Meat Sauce",
                    Description = "Penne noodles with a tomato-based meat sauce.",
                    Ingredients = "Penne noodles, ground beef, tomato juice, onion, salt and pepper",
                    Instructions = "Cook noodles. Brown beef and then mix with tomato juice, onion, salt and pepper.",
                    PreparationTime = 15,
                    CookingTime = 30,
                    Servings = 6,
                    ImageUrl = "https://images.unsplash.com/photo-1621996346565-e3dbc646d9a9?w=500&auto=format&fit=crop&q=60",
                    UserId = users[0].Id,
                    CategoryId = categories[2].Id
                },
                new Recipe
                {
                    Name = "Grilled Chicken Salad",
                    Description = "Delicious grilled chicken salad.",
                    Ingredients = "Chicken, Romaine lettuce, tomatoes, and salad croutons",
                    Instructions = "Season chicken and then grill. Place lettuce in a bowl and then put grilled chicken on top along with tomatoes and croutons.",
                    PreparationTime = 15,
                    CookingTime = 25,
                    Servings = 6,
                    ImageUrl = "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=500&auto=format&fit=crop&q=60",
                    UserId = users[1].Id,
                    CategoryId = categories[1].Id
                },
                new Recipe
                {
                    Name = "Glazed Salmon",
                    Description = "Salmon with a tangy citrus glaze.",
                    Ingredients = "Salmon, lemon, orange, sugar, salt, and pepper",
                    Instructions = "Make glaze by mixing lemon, orange, sugar, salt and pepper.  Put salmon on grill and lightly coat with glaze. Add more glaze as needed.",
                    PreparationTime = 15,
                    CookingTime = 30,
                    Servings = 6,
                    ImageUrl = "https://images.unsplash.com/photo-1519708227418-c8fd9a32b7a2?w=500&auto=format&fit=crop&q=60",
                    UserId = users[0].Id,
                    CategoryId = categories[2].Id
                },
                new Recipe
                {
                    Name = "Chocolate Brownies",
                    Description = "Gooey chocolate brownies.",
                    Ingredients = "Sugar, flour, butter, eggs, cocoa powder, vanilla, baking powder, and salt",
                    Instructions = "Whisk sugar, flour, melted butter, eggs, cocoa powder, vanilla, baking powder, and salt in a large bowl until combined.  Spread batter into a greased pan and cook at 350 degrees F for 25 minutes.",
                    PreparationTime = 10,
                    CookingTime = 25,
                    Servings = 10,
                    ImageUrl = "https://images.unsplash.com/photo-1636743715220-d8f8dd900b87?w=500&auto=format&fit=crop&q=60",
                    UserId = users[0].Id,
                    CategoryId = categories[3].Id
                },
                new Recipe
                {
                    Name = "Apple Pie",
                    Description = "Sweet apple pie with flaky crust.",
                    Ingredients = "Pie crust, apples, sugar, flour, cinnamon, salt, nutmeg, and butter",
                    Instructions = "Combine apples, sugar, flour, cinnamon, salt, and nutmeg in a bowl. Put everything into prepared pie crust. Cut holes in top crust to allow venting.",
                    PreparationTime = 25,
                    CookingTime = 50,
                    Servings = 8,
                    ImageUrl = "https://images.unsplash.com/photo-1628815871005-a860e5905628?w=500&auto=format&fit=crop&q=60",
                    UserId = users[1].Id,
                    CategoryId = categories[3].Id
                },
                new Recipe
                {
                    Name = "Belgian Waffles",
                    Description = "A yummy breakfast dish that is thick and crispy on the outside and fluffy on the inside.",
                    Ingredients = "Flour, sugar, baking powder, baking soda, salt, three large eggs, milk, melted butter, vanilla extract, cinnamon, and nutmeg",
                    Instructions = "In a large bowl, whisk together flour, cornstarch, sugar, baking powder, baking soda, and salt until evenly combined.  In another bowl, whisk the egg yolks with milk, melted butter, and vanilla extract until smooth.  Pour the wet mixture into the dry ingredients and stir gently until just combined. The batter should remain slightly lumpy to avoid overmixing. Pour the batter into the hot waffle iron (about ¾ cup per waffle, depending on size) and cook until golden brown and crisp, usually 4-5 minutes.",
                    PreparationTime = 15,
                    CookingTime = 5,
                    Servings = 12,
                    ImageUrl = "https://images.unsplash.com/photo-1568051243851-f9b136146e97?w=500&auto=format&fit=crop&q=60",
                    UserId = users[0].Id,
                    CategoryId = categories[0].Id
                },
                new Recipe
                {
                    Name = "Rotini pasta salad",
                    Description = "A savory tomato-based sauce mixed with rotini pasta noodles.",
                    Ingredients = "Rotini pasta, tomato sauce and sliced tomatoes",
                    Instructions = "Cook pasta and then mix with tomato sauce.  Top with sliced tomatoes.",
                    PreparationTime = 15,
                    CookingTime = 5,
                    Servings = 12,
                    ImageUrl = "https://images.unsplash.com/photo-1608897013039-887f21d8c804?w=500&auto=format&fit=crop&q=60",
                    UserId = users[1].Id,
                    CategoryId = categories[1].Id
                }
            };

            context.Recipes.AddRange(recipes);
            context.SaveChanges();
        }
    }
}