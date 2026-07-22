/*
 * ===========================================================
 * ChefConnect - CSE 325 Group Project
 *
 * File: ChefConnectContext.cs
 * Contributors:
 *   - Godfred Sefa Aboagye
 *   - Kamohelo Godfrey Mejaele
 *
 * Purpose:
 * Configures the Entity Framework Core DbContext, database
 * tables, relationships, constraints, and indexes for the
 * ChefConnect application.
 * ===========================================================
 */


using ChefConnect.Models;
using Microsoft.EntityFrameworkCore;

namespace ChefConnect.Data
{
    /// <summary>
    /// The ChefConnectContext class represents the application's database session.
    /// It is responsible for querying and saving data to the database using
    /// Entity Framework Core.
    /// </summary>
    public class ChefConnectContext : DbContext
    {
        /// <summary>
        /// Constructor used by ASP.NET Core Dependency Injection.
        /// </summary>
        /// <param name="options">Database configuration options.</param>
        public ChefConnectContext(DbContextOptions<ChefConnectContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Represents the Users table in the database.
        /// </summary>
        public DbSet<User> Users { get; set; } = default!;

        /// <summary>
        /// Represents the Recipes table.
        /// </summary>
        public DbSet<Recipe> Recipes { get; set; } = default!;

        /// <summary>
        /// Represents the Categories table.
        /// </summary>
        public DbSet<Category> Categories { get; set; } = default!;

        /// <summary>
        /// Configures relationships, constraints, and indexes.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // -----------------------------
            // User Configuration
            // -----------------------------

            // Usernames must be unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Emails must be unique
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // -----------------------------
            // Category Configuration
            // -----------------------------

            // Category names should be unique
            modelBuilder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique();

            // -----------------------------
            // Recipe Relationships
            // -----------------------------

            // One User -> Many Recipes
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.User)
                .WithMany(u => u.Recipes)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One Category -> Many Recipes
            modelBuilder.Entity<Recipe>()
                .HasOne(r => r.Category)
                .WithMany(c => c.Recipes)
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}