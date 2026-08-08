# ChefConnect

ChefConnect is a .NET Blazor web application developed as a group project for CSE 325 at BYU-Idaho.

The application provides a centralized platform where users can discover, create, manage, and save recipes. Users can create accounts, log in securely, manage their own recipes, and mark recipes as favorites.

---

## Group

**Group Name:** CSE325 Group1

---

## Team Members

- Blake Wayne Ostler
- Robert Yamashita
- Dereck Romero Moscoso
- Samuel Jericho Revillo Gomez
- Gideon Komla Agbavor
- Edward John Griffeth
- Mukendji Josaphat Ngandu
- Sophia Akwero
- Godfred Sefa Aboagye
- Kamohelo Godfrey Mejaele
- Jayce Odin Nephi Brown

---

## Tech Stack

- .NET 10
- Blazor
- ASP.NET Core Identity
- Entity Framework Core
- SQLite
- Bootstrap
- Bootstrap Icons
- Azure

---

## Project Overview

ChefConnect is designed to make recipe management simple and accessible.

Users can:

- Register for an account
- Log in and log out securely
- Browse available recipes
- View detailed recipe information
- Create their own recipes
- Edit recipes they own
- Delete recipes they own
- Add recipes to their favorites
- Remove recipes from their favorites
- View recipes associated with their account
- Organize recipes using categories

The application uses ASP.NET Core Identity to manage authentication and Entity Framework Core to manage communication with the SQLite database.

---

## Project Status

### Completed

The following features have been implemented:

- Blazor project structure
- GitHub repository and branch protection
- Entity Framework Core database configuration
- SQLite database integration
- Database schema for users, recipes, categories, and favorites
- EF Core migrations
- Database seed data
- ASP.NET Core Identity authentication
- User registration
- User login
- User logout
- Authentication-aware navigation
- Recipe listing
- Recipe details page
- Recipe creation
- Recipe editing
- Recipe deletion
- Recipe ownership protection
- Recipe categories
- Recipe favorites
- Favorite/unfavorite functionality
- Form validation
- Delete confirmation
- Bootstrap styling
- Bootstrap Icons
- Initial home page design and styling

### In Progress

The team is continuing to improve:

- Home page design and user experience
- Recipe search
- Category filtering
- User profile functionality
- Additional application styling
- Final deployment and testing

---

## Database Design

ChefConnect uses Entity Framework Core with SQLite.

The main database entities include:

### Users

Stores authenticated application users managed through ASP.NET Core Identity.

Users can create multiple recipes and maintain their own favorite recipes.

### Recipes

Stores recipe information including:

- Name
- Description
- Ingredients
- Instructions
- Preparation time
- Cooking time
- Servings
- Image URL
- Creation date
- Recipe owner
- Category

### Categories

Stores recipe categories such as:

- Breakfast
- Lunch
- Dinner
- Dessert

A category can contain multiple recipes.

### Favorites

Stores the relationship between users and recipes that they have marked as favorites.

---

## Security

ChefConnect uses ASP.NET Core Identity for authentication and authorization.

The application includes:

- Secure password hashing through ASP.NET Core Identity
- Authentication cookies
- Login and registration
- Secure POST-based logout
- Antiforgery protection
- Recipe ownership verification
- Protected recipe editing
- Protected recipe deletion

Users can only edit or delete recipes that they own.

---

## Recipe Management

### Create a Recipe

Authenticated users can create a recipe by providing:

1. Recipe name
2. Description
3. Category
4. Ingredients
5. Instructions
6. Preparation time
7. Cooking time
8. Number of servings
9. Optional image URL

The application associates the recipe with the authenticated user.

### Edit a Recipe

Only the user who created a recipe can edit it.

### Delete a Recipe

Deleting a recipe requires confirmation before the operation is completed.

### Favorites

Authenticated users can favorite recipes and remove recipes from their favorites.

---

## Setup

### 1. Clone the Repository

```bash
git clone https://github.com/gideonagbavor8/CSE325-Group1.git