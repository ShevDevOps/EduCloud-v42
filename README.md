# EduCloud-v42

## Introduction

Welcome to EduCloud-v42! This project is an ASP.NET Core web application built on .NET 8. It utilizes Entity Framework Core for data access with a SQLite database, following the Model-View-Controller (MVC) pattern. This document provides instructions for developers to set up and run the project locally.

## Technologies Used

*   **.NET 8.0**: The underlying framework for the application.
*   **ASP.NET Core**: For building the web application.
*   **Entity Framework Core**: For object-relational mapping (ORM).
*   **SQLite**: The database engine used for local development.
*   **MVC (Model-View-Controller)**: The architectural pattern used for the application structure.
*   **Google.Apis.Auth 1.72.0**: The Google APIs Client Library is a runtime client for working with Google services

## Prerequisites

Before you begin, ensure you have the following installed on your machine:

*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Git](https://git-scm.com/downloads)
*   A code editor or IDE of your choice, such as:
    *   [Visual Studio 2022](https://visualstudio.microsoft.com/)
    *   [Visual Studio Code](https://code.visualstudio.com/)

## Getting Started

Follow these steps to get a local instance of the project up and running.

### 1. Clone the Repository

Open your terminal or command prompt and clone the repository to your local machine:

```bash
git clone <repository-url>
cd EduCloud-v42
```

### 2. Open the Project

You can open the project in your preferred IDE.

*   **For Visual Studio:** Open the `EduCloud-v42.sln` solution file.
*   **For VS Code:** Open the root `EduCloud-v42` folder.

### 3. Restore Dependencies

The .NET dependencies should be restored automatically by Visual Studio. If you are using the command line or need to do it manually, run the following command from the root directory (`EduCloud-v42`):

```bash
dotnet restore
```

### 4. Database Migrations

The project uses Entity Framework Core to manage the database schema. The database file (`siteData.db`) is included in the repository, but if you need to apply new migrations or recreate the database, you can use the following commands from the `EduCloud-v42/EduCloud-v42` directory:

```bash
# This command will apply any pending migrations to the database.
dotnet ef database update
```

If you need to create new migrations based on changes to the `Model` classes, use:
```bash
# Replace <MigrationName> with a descriptive name for your migration.
dotnet ef migrations add <MigrationName>
```

### 5. Run the Application

You can now run the application.

*   **From Visual Studio:** Press `F5` or the "Start Debugging" button.
*   **From the command line:** Navigate to the `EduCloud-v42/EduCloud-v42` directory and run:

```bash
dotnet run
```

The application will be available at `https://localhost:port` and `http://localhost:port`, where `port` is specified in the `Properties/launchSettings.json` file.

## Project Structure

The project follows a standard ASP.NET Core MVC structure:

*   `Controllers/`: Contains the controller classes that handle user requests and return responses.
*   `Models/`: Contains the entity classes that represent the application's data structure.
*   `Views/`: Contains the `.cshtml` files for the user interface.
*   `Services/`: Contains business logic and services used by the controllers.
*   `Migrations/`: Contains the Entity Framework Core database migration files.
*   `wwwroot/`: Contains static assets like CSS, JavaScript, and images.
*   `appsettings.json`: Configuration file for the application, including the database connection string.

## Configuration

The main configuration file is `appsettings.json`. The database connection string for the SQLite database is located here.

Example `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "DataSource=siteData.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## Contributing

Contributions are welcome! If you find any issues or have suggestions for improvements, please feel free to open an issue or submit a pull request.

---
Happy coding!