# Promotion Tasks Service

A .NET 8.0 Web API service for managing music release promotion tasks.

## Demo and Architectural decisions

The project uses:

- Entity Framework Core for data access
- SQLite as the database
- Swagger/OpenAPI for API documentation
- xUnit for testing
- Moq for mocking in tests

I decided to keep the API and Frontend separate rather than have them in a monorepo, so that they could be deployed separately with their own pipelines. This would enable them to be scaled in dependently too. It does result in Cors complications in this scenario as I'm just using Azure's default hostnames, but I've mitigated that by setting the Cors policy.

After starting with a simple tasks list I decided that tasks should also be tied to a user entity, so that there's some context.
Then I realised that it made sense that the promotion tasks would be against a release, as a user could have many releases each with their own task lists.
I added a task list template table into the database to serve as a set list of tasks that a release always starts with. This template list could be editied by an admin app for example.
I also added priority to the tasks, with the view that the user may want to reorder them as well as update their status.

The code is using GitHub as source control, and automatically deploys out to Azure app services when there is a commit.
It runs the unit tests in the build pipeline too.
That's not a productionised pipeline (needs quality gates, separate environments, pipelines for manually triggering deployments to Production), but fine for demo purposes.

The Swagger endpoint for this on Azure is
https://promotiontasksdemo-api-bvdudegdachcdddb.uksouth-01.azurewebsites.net/swagger/index.html

Lastly, I added in an analytics service to give data on:

- Overall completion percentage across all tasks
- Completion percentage per user
- Average completion percentage per user
- Completion percentage per release
- Average completion percentage per release

### Unfinished areas

I didn't get time to implement proper authentication. So the backend API is currently fully open.
This would be the next thing to tackle with more time.

## Getting Started

1. Clone the repository
2. Ensure you have .NET 8.0 SDK installed
3. Run the following commands:
   ```bash
   dotnet restore
   dotnet build
   dotnet run
   ```
4. The API will be available at `http://localhost:5110` / `https://localhost:5111`
5. Swagger documentation is available at `/swagger` endpoint
6. To run tests run the command 'dotnet test'
7. Note, for the purpose of the demo I'm returning all data for the User (User, Releases & Release tasks) in the single call
   GET `/api/users/{id}`
   The other endpoints are there for managing data and updating tasks.

## Project Structure

```
PromotionTasksService/
├── Controllers/           # API endpoints
│   ├── AnalyticsController.cs
│   ├── PromotionTasksController.cs
│   ├── ReleaseController.cs
│   └── UserController.cs
├── Models/               # Data models
│   ├── Analytics/
│   │   ├── TaskCompletionAnalytics.cs
│   │   ├── UserCompletionAnalytics.cs
│   │   └── ReleaseCompletionAnalytics.cs
│   ├── User.cs
│   ├── Release.cs
│   ├── ReleaseTask.cs
│   ├── ReleaseType.cs
│   ├── TaskPriority.cs
│   ├── PromotionTask.cs
│   └── PromotionTaskStatus.cs
├── Services/            # Business logic
│   ├── AnalyticsService.cs
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
- PUT `/api/promotiontasks/{id}/priority` - Update task priority

### Analytics

- GET `/api/analytics/completion` - Get overall task completion analytics
  - Returns overall completion percentage, average per user, and average per release
- GET `/api/analytics/completion/users` - Get task completion analytics per user
  - Returns a list of users with their respective completion percentages
- GET `/api/analytics/completion/releases` - Get task completion analytics per release
  - Returns detailed stats for each release including completion percentage, total tasks, and completed tasks
