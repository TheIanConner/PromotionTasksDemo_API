# Promotion Tasks Service

A .NET 8.0 Web API service for managing music release promotion tasks.

## Getting Started

1. Clone the repository
2. Ensure you have .NET 8.0 SDK installed
3. Run the following commands:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```
4. The API will be available at `http://localhost:5000` / `https://localhost:5001`
5. Swagger documentation is available at `/swagger` endpoint
6. To run tests run the command 'dotnet test'
7. Note, for the purpose of the demo I'm returning all data for the User (User, Releases & Release tasks) in the call
   GET `/api/users/{id}`

## Development

The project uses:

- Entity Framework Core for data access
- SQLite as the database
- Swagger/OpenAPI for API documentation
- xUnit for testing
- Moq for mocking in tests

## Project Structure

```
PromotionTasksService/
├── Controllers/           # API endpoints
│   ├── PromotionTasksController.cs
│   ├── ReleaseController.cs
│   └── UserController.cs
├── Models/               # Data models
│   ├── User.cs
│   ├── Release.cs
│   ├── PromotionTask.cs
│   └── PromotionTaskStatus.cs
├── Services/            # Business logic
│   ├── PromotionTasksService.cs
│   ├── ReleaseService.cs
│   ├── UserService.cs
│   └── DatabaseSeeder.cs
├── Data/               # Data access layer
│   └── ApplicationDbContext.cs
└── Tests/             # Unit tests
```

## Database Structure

The application uses SQLite as its database (for demo purposes) and follows a hierarchical structure:

### Users Table

- Primary entity representing artists or users
- Fields:
  - `UserId` (Primary Key)
  - `Name` (Required, max 50 chars)
  - `CreatedDate` (UTC timestamp)
  - `LastActiveDate` (UTC timestamp)
  - `Deleted` (Soft delete flag)
- One-to-Many relationship with Releases

### Releases Table

- Represents music releases (singles, EPs, albums, etc.)
- Fields:
  - `ReleaseId` (Primary Key)
  - `UserId` (Foreign Key to Users)
  - `Title` (Required, max 200 chars)
  - `Type` (Enum: Single, EP, Album, Mixtape)
  - `ReleaseDate` (Required)
  - `Description` (Optional, max 1000 chars)
  - `Deleted` (Soft delete flag)
- Many-to-One relationship with Users
- One-to-Many relationship with PromotionTasks

### PromotionTasks Table

- Represents tasks related to promoting a release
- Fields:
  - `TaskId` (Primary Key)
  - `ReleaseId` (Foreign Key to Releases)
  - `Status` (Enum: ToDo, InProgress, Done)
  - `Priority` (Enum: Low, Medium, High, Urgent)
  - `Description` (Required, max 1000 chars)
  - `DueDate` (Optional)
  - `Deleted` (Soft delete flag)
- Many-to-One relationship with Releases

## API Endpoints

### Users

- GET `/api/users` - Get all users
- GET `/api/users/{id}` - Get user by ID
- POST `/api/users` - Create new user
- PUT `/api/users/{id}` - Update user

### Releases

- GET `/api/releases` - Get all releases
- GET `/api/releases/{id}` - Get release by ID
- POST `/api/releases` - Create new release
- PUT `/api/releases/{id}` - Update release

### Promotion Tasks

- GET `/api/promotiontasks` - Get all tasks
- GET `/api/promotiontasks/{id}` - Get task by ID
- POST `/api/promotiontasks` - Create/Update task
- PUT `/api/promotiontasks/{id}/status` - Update task status
