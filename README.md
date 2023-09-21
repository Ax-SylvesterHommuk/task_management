# task_management
Improved version of the older one with C# ASP.NET 7.0 backend instead of Nodejs.

## Getting Started

Before you start, make sure you have the following prerequisites installed on your machine:

- [.NET 7.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0): You'll need this to run the backend server.
- [Node.js](https://nodejs.org/): Required for building the frontend.

## Instructions
- First follow inside the `frontend` folder and run `npm install` to install all the dependencies.
- Then run `npm run build` to build the frontend.

- Copy the contents of the `frontend/dist/` folder and paste them into the `backend/wwwroot` folder.
- Setup MySQL database by opening `appsettings.json` and changing the `DefaultConnection` string to your database connection string. Import the `task_management_structure.sql` file into your database.
- You can tweak other settings in the `appsettings.json` file as well. (optional)
- Now just run `dotnet run` inside the `backend` folder and the server should start at `http://localhost:80`.

- Swagger UI is available at `http://localhost:80/swagger/index.html`.

# Features
- [x] View all tasks
- [x] Create a task
- [x] Delete a task
- [x] Edit a task

# Authentication
- [x] Register
- [x] Login
- [x] Logout

# Database
- [x] MySQL (must be installed on your machine)

# Security
- [x] CORS enabled by default (http://localhost:80)