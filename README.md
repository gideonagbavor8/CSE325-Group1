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
- Delete confirmation shown in a modal dialog
- Bootstrap styling
- Bootstrap Icons
- Home page design and styling
- Recipe search by name and category
- Category filtering
- Recipe image upload, alongside the existing image URL
- Frequently asked questions section on the home page
- User profile page
- Deployment to Azure App Service

### In Progress

The team is continuing to improve:

- Mobile and small screen layout
- Additional application styling
- A page listing the recipes a user has favorited

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
9. An optional image

The application associates the recipe with the authenticated user.

### Recipe Images

An image is optional, and there are two ways to supply one:

- **Upload from the user's device.** JPG, JPEG, PNG, and WEBP files of
  up to 5 MB are accepted. Uploaded files are given a generated name so
  the name chosen by the user is never used as a file path.
- **An externally hosted image URL** beginning with `http://` or
  `https://`.

If both are supplied, the uploaded image is the one that is saved.

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
cd CSE325-Group1
```

### 2. Restore and Build

```bash
dotnet restore
dotnet build
```

### 3. Run the Application

```bash
dotnet run
```

The application is then available at the URL shown in the terminal
(https://localhost:7146 by default).

The SQLite database is created automatically the first time the
application starts. Entity Framework Core applies the migrations and
`DbInitializer` adds the sample categories, users, and recipes.

Seeded accounts use the password `Passw0rd!`:

- gsefa@example.com
- kamohelo@example.com

---

## Testing

Automated tests live in `Tests/` and are written with
[bUnit](https://bunit.dev/) and xUnit. They render the real Razor
components against an in-memory SQLite database, so they exercise the
same code the application runs.

Run them with:

```bash
dotnet test Tests/ChefConnect.Tests.csproj
```

The tests cover:

- **Authorization.** A signed-out visitor, and a user who does not own
  a recipe, cannot delete or edit it. These are checked in the service
  as well as the page, because hiding a button is not protection.
- **Recipe images.** Uploading a file, supplying a URL, supplying both
  (the upload wins), supplying neither, rejecting the wrong file type,
  and rejecting a file over 5 MB.
- **Image storage.** Uploaded files are given a generated name, the
  name chosen by the user is never used as a path, and a rejected file
  is never written to disk.
- **Recipe display.** Cards and the details page show external image
  URLs, uploaded images, and the placeholder when there is no image.
- **Search and favorites.** Searching finds a recipe whether or not it
  has been favorited, and favoriting from the home page does not remove
  a recipe from the results.
- **Confirmation and messages.** Deleting asks for confirmation first
  and removes nothing until it is confirmed, and the confirmation
  message clears itself.

The test project is separate from the application and is never included
in a deployment. `ChefConnect.csproj` excludes the `Tests` folder, which
is required because the Web SDK otherwise compiles every `.cs` file
below the project folder into the web application.

---

## Deployment

ChefConnect is deployed to **Azure App Service (Linux)**.

**Live site:** https://chefconnect-cse325g1.azurewebsites.net

### Azure Resources

| Resource | Name | Notes |
| --- | --- | --- |
| Resource group | `rg-chefconnect` | Holds every resource for the project |
| App Service plan | `plan-chefconnect` | B1 Basic, Linux, France Central |
| Web app | `chefconnect-cse325g1` | .NET 10 runtime |

France Central is used because the Azure for Students subscription
restricts deployments to a small set of regions, none of them in the
United States.

### Required Site Configuration

Blazor Server keeps a SignalR connection open for the whole session, so
the site needs the following:

- **WebSockets enabled**, otherwise the connection falls back to long
  polling and the interface feels slow
- **Always On enabled**, otherwise the application is unloaded while
  idle and open connections are dropped
- **HTTPS only**, because the authentication cookie is issued with the
  `secure` flag and is not sent over plain HTTP
- **Startup command** `dotnet /home/site/wwwroot/ChefConnect.dll`

### How the Database Survives a Deployment

Everything inside the deployed application folder
(`/home/site/wwwroot`) is replaced each time the site is deployed, so
the database and the uploaded images are kept outside it, in Azure's
persistent `/home` storage.

Two application settings control this:

| Setting | Value |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | `Data Source=/home/data/app.db` |
| `RecipeImages__StoragePath` | `/home/data/uploads/recipes` |

Neither setting exists locally, so a development machine keeps using
`app.db` in the project folder and `wwwroot/uploads/recipes` as before.

SQLite creates the database file but not the folder holding it, so
`Program.cs` creates that folder during startup.

### Deploying an Update

```powershell
dotnet publish -c Release -o ./publish
Remove-Item ./publish.zip -Force -ErrorAction SilentlyContinue
tar -a -c -f "$PWD\publish.zip" -C publish .
az webapp stop --name chefconnect-cse325g1 --resource-group rg-chefconnect
az webapp deploy --resource-group rg-chefconnect --name chefconnect-cse325g1 --src-path publish.zip --type zip --clean true
az webapp start --name chefconnect-cse325g1 --resource-group rg-chefconnect
```

Three details in that sequence are easy to get wrong:

- **Use `tar`, not `Compress-Archive`.** Windows PowerShell writes zip
  entries with backslashes, which Linux rejects, and the deployment
  fails partway through with `Invalid argument (22)` for every file.
- **Do not use `az webapp up`.** It uploads the entire project folder,
  including `bin`, `obj`, and `publish`, which leaves several copies of
  the application in `wwwroot` and stops the site from starting.
- **Stop the site first.** Deploying to a running site that is
  restarting can return `HTTP 502` from the deployment service.

### Limitations

- **Run one instance only.** SQLite is a single file on shared storage
  and can be corrupted by more than one instance writing to it.
- **There are no automatic backups.** Download `/home/data/app.db`
  through the Kudu console before anything important.
- **The site is hosted in Europe**, so there is a noticeable delay on
  each interaction from North America.

### Removing the Deployment

Deleting the resource group removes every resource and stops all
charges:

```bash
az group delete --name rg-chefconnect
```