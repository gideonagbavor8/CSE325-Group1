# CSE 325 Group Project - ChefConnect

A .NET Blazor web application developed as a group project for CSE 325 at BYU-Idaho.

## Group
- Group Name: CSE325 Group1

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

## Tech Stack
- .NET Blazor
- ASP.NET Core Identity (authentication)
- Entity Framework Core
- SQLite (database)
- Azure (deployment)

## Project Status
🚧 In progress

Completed so far:
- Blazor project template and GitHub repo with branch protection rules
- Database schema design (Users, Recipes, Categories) with EF Core migrations
- Standardized on SQLite as the single database provider across the app
- Seed data for initial recipes, categories, and users
- Authentication scaffolding with ASP.NET Core Identity, including a working login page
- Initial home page design and styling

In progress:
- User registration and logout flow
- Recipe CRUD (create, edit, delete)
- Recipe search and category filtering
- Favorites feature
- User profile page

## Setup
1. Clone the repository:

git clone https://github.com/gideonagbavor8/CSE325-Group1.git

2. Navigate into the project folder:

cd CSE325-Group1

3. Restore dependencies:

dotnet restore

4. Apply database migrations:

dotnet ef database update

5. Run the application:

dotnet watch

6. Open the app in your browser at the URL shown in the terminal (typically `http://localhost:5000`).

## User Guide
Documentation will be added as features are completed. This section will include instructions on how to register, log in, create and search recipes, and manage favorites once those features are implemented.
