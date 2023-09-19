# task_management
Improved version of the older one with C# backend instead of Nodejs.

# Getting Started
- Make sure dotnet 7.0 SDK is installed on your machine.
[Download](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

## Instructions
- First follow inside the `frontend` folder and run `npm install` to install all the dependencies.
- Then run `npm run build` to build the frontend.

- Copy the contents of the `frontend/dist/` folder and paste them into the `backend/wwwroot` folder.
- Now just run `dotnet run` inside the `backend` folder and the server should start at `localhost:3000`.

# Features
- [x] View all tasks
- [x] Create a task
- [x] Update a task
- [x] Delete a task
- [x] Edit a task

# Authentication
- [x] Register
- [x] Login
- [x] Logout

# Database
- [x] SQLite3 (automatically comes with the backend)