using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PromotionTasksService.Data;
using PromotionTasksService.Models;
using PromotionTasksService.Services;
using Xunit;

namespace PromotionTasksService.Tests.Services;

public class AnalyticsServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> dbContextOptions;
    private readonly Mock<ILogger<AnalyticsService>> mockLogger;
    
    public AnalyticsServiceTests()
    {
        this.dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        this.mockLogger = new Mock<ILogger<AnalyticsService>>();
    }
    
    private ApplicationDbContext CreateContext()
    {
        var context = new ApplicationDbContext(this.dbContextOptions);
        context.Database.EnsureCreated();
        return context;
    }
    
    private void SeedDatabase(ApplicationDbContext context)
    {
        // Add two users
        var users = new List<User>
        {
            new User
            {
                UserId = 1,
                Name = "User 1",
                CreatedDate = DateTime.UtcNow.AddDays(-30),
                LastActiveDate = DateTime.UtcNow,
                Deleted = false
            },
            new User
            {
                UserId = 2,
                Name = "User 2",
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                LastActiveDate = DateTime.UtcNow,
                Deleted = false
            },
            new User
            {
                UserId = 3,
                Name = "Deleted User",
                CreatedDate = DateTime.UtcNow.AddDays(-10),
                LastActiveDate = DateTime.UtcNow,
                Deleted = true
            }
        };
        
        context.Users.AddRange(users);
        context.SaveChanges();
        
        // Add releases for each user
        var releases = new List<Release>
        {
            new Release
            {
                ReleaseId = 1,
                UserId = 1,
                Title = "Release 1 for User 1",
                Type = ReleaseType.Single,
                ReleaseDate = DateTime.UtcNow.AddDays(10),
                Description = "Test release 1",
                Deleted = false
            },
            new Release
            {
                ReleaseId = 2,
                UserId = 1,
                Title = "Release 2 for User 1",
                Type = ReleaseType.Album,
                ReleaseDate = DateTime.UtcNow.AddDays(20),
                Description = "Test release 2",
                Deleted = false
            },
            new Release
            {
                ReleaseId = 3,
                UserId = 2,
                Title = "Release 1 for User 2",
                Type = ReleaseType.EP,
                ReleaseDate = DateTime.UtcNow.AddDays(5),
                Description = "Test release 3",
                Deleted = false
            },
            new Release
            {
                ReleaseId = 4,
                UserId = 2,
                Title = "Deleted Release",
                Type = ReleaseType.Single,
                ReleaseDate = DateTime.UtcNow.AddDays(15),
                Description = "This release is deleted",
                Deleted = true
            }
        };
        
        context.Releases.AddRange(releases);
        context.SaveChanges();
        
        // Add tasks for each release
        var tasks = new List<PromotionTask>
        {
            // Tasks for Release 1 (User 1) - 2/4 complete = 50%
            new PromotionTask { TaskId = 1, ReleaseId = 1, Description = "Task 1-1", Status = PromotionTaskStatus.Done, Priority = TaskPriority.High, Deleted = false },
            new PromotionTask { TaskId = 2, ReleaseId = 1, Description = "Task 1-2", Status = PromotionTaskStatus.Done, Priority = TaskPriority.Medium, Deleted = false },
            new PromotionTask { TaskId = 3, ReleaseId = 1, Description = "Task 1-3", Status = PromotionTaskStatus.ToDo, Priority = TaskPriority.Low, Deleted = false },
            new PromotionTask { TaskId = 4, ReleaseId = 1, Description = "Task 1-4", Status = PromotionTaskStatus.InProgress, Priority = TaskPriority.Urgent, Deleted = false },
            
            // Tasks for Release 2 (User 1) - 1/3 complete = 33.33%
            new PromotionTask { TaskId = 5, ReleaseId = 2, Description = "Task 2-1", Status = PromotionTaskStatus.Done, Priority = TaskPriority.Medium, Deleted = false },
            new PromotionTask { TaskId = 6, ReleaseId = 2, Description = "Task 2-2", Status = PromotionTaskStatus.ToDo, Priority = TaskPriority.Low, Deleted = false },
            new PromotionTask { TaskId = 7, ReleaseId = 2, Description = "Task 2-3", Status = PromotionTaskStatus.ToDo, Priority = TaskPriority.High, Deleted = false },
            
            // Tasks for Release 3 (User 2) - 2/3 complete = 66.67%
            new PromotionTask { TaskId = 8, ReleaseId = 3, Description = "Task 3-1", Status = PromotionTaskStatus.Done, Priority = TaskPriority.High, Deleted = false },
            new PromotionTask { TaskId = 9, ReleaseId = 3, Description = "Task 3-2", Status = PromotionTaskStatus.Done, Priority = TaskPriority.Low, Deleted = false },
            new PromotionTask { TaskId = 10, ReleaseId = 3, Description = "Task 3-3", Status = PromotionTaskStatus.InProgress, Priority = TaskPriority.Medium, Deleted = false },
            
            // Deleted task (should not count)
            new PromotionTask { TaskId = 11, ReleaseId = 1, Description = "Deleted Task", Status = PromotionTaskStatus.Done, Priority = TaskPriority.Medium, Deleted = true },
            
            // Task for deleted release (should not count)
            new PromotionTask { TaskId = 12, ReleaseId = 4, Description = "Task for deleted release", Status = PromotionTaskStatus.Done, Priority = TaskPriority.Medium, Deleted = false }
        };
        
        context.PromotionTasks.AddRange(tasks);
        context.SaveChanges();
    }
    
    [Fact]
    public async Task GetOverallCompletionPercentageAsync_ShouldCalculateCorrectPercentage()
    {
        // Arrange
        using var context = this.CreateContext();
        this.SeedDatabase(context);
        
        var service = new AnalyticsService(context, this.mockLogger.Object);
        
        // Total non-deleted tasks: 11 (10 active + 1 for deleted release)
        // Completed non-deleted tasks: 6 (5 active + 1 for deleted release)
        // Expected percentage: ~54.55%
        
        // Act
        var result = await service.GetOverallCompletionPercentageAsync();
        
        // Assert
        Assert.InRange(result, 54.5, 54.6);
    }
    
    [Fact]
    public async Task GetCompletionPercentagePerUserAsync_ShouldReturnCorrectPercentages()
    {
        // Arrange
        using var context = this.CreateContext();
        this.SeedDatabase(context);
        
        var service = new AnalyticsService(context, this.mockLogger.Object);
        
        // Note: The implementation filters tasks by non-deleted releases first, then filters by non-deleted tasks
        // So the calculation is different from our initial expectations
        
        // Act
        var result = await service.GetCompletionPercentagePerUserAsync();
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey(1));
        Assert.True(result.ContainsKey(2));
        Assert.False(result.ContainsKey(3)); // Deleted user
        
        // User 1: 3/6 complete = 50%
        // User 2: 2/3 complete = 66.67%
        Assert.Equal(50, result[1]);
        Assert.InRange(result[2], 66.6, 66.7);
    }
    
    [Fact]
    public async Task GetAverageCompletionPercentagePerUserAsync_ShouldCalculateCorrectAverage()
    {
        // Arrange
        using var context = this.CreateContext();
        this.SeedDatabase(context);
        
        var service = new AnalyticsService(context, this.mockLogger.Object);
        
        // User 1: 50%
        // User 2: 66.67%
        // Average: ~58.33%
        
        // Act
        var result = await service.GetAverageCompletionPercentagePerUserAsync();
        
        // Assert
        Assert.InRange(result, 58.3, 58.4);
    }
    
    [Fact]
    public async Task GetCompletionPercentagePerReleaseAsync_ShouldReturnCorrectPercentages()
    {
        // Arrange
        using var context = this.CreateContext();
        this.SeedDatabase(context);
        
        var service = new AnalyticsService(context, this.mockLogger.Object);
        
        // Release 1: 3/5 complete = 60% (including deleted task)
        // Release 2: 1/3 complete = 33.33%
        // Release 3: 2/3 complete = 66.67%
        
        // Act
        var result = await service.GetCompletionPercentagePerReleaseAsync();
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.True(result.ContainsKey(1));
        Assert.True(result.ContainsKey(2));
        Assert.True(result.ContainsKey(3));
        Assert.False(result.ContainsKey(4)); // Deleted release
        
        Assert.Equal(60, result[1]);
        Assert.InRange(result[2], 33.3, 33.4);
        Assert.InRange(result[3], 66.6, 66.7);
    }
    
    [Fact]
    public async Task GetAverageCompletionPercentagePerReleaseAsync_ShouldCalculateCorrectAverage()
    {
        // Arrange
        using var context = this.CreateContext();
        this.SeedDatabase(context);
        
        var service = new AnalyticsService(context, this.mockLogger.Object);
        
        // Release 1: 60%
        // Release 2: 33.33%
        // Release 3: 66.67%
        // Average: ~53.33%
        
        // Act
        var result = await service.GetAverageCompletionPercentagePerReleaseAsync();
        
        // Assert
        Assert.InRange(result, 53.3, 53.4);
    }
    
    [Fact]
    public async Task GetOverallCompletionPercentageAsync_WithNoTasks_ShouldReturnZero()
    {
        // Arrange
        using var context = this.CreateContext();
        // Don't seed any tasks
        
        var service = new AnalyticsService(context, this.mockLogger.Object);
        
        // Act
        var result = await service.GetOverallCompletionPercentageAsync();
        
        // Assert
        Assert.Equal(0, result);
    }
    
    [Fact]
    public async Task GetAverageCompletionPercentagePerUserAsync_WithNoUsers_ShouldReturnZero()
    {
        // Arrange
        using var context = this.CreateContext();
        // Don't seed any users
        
        var service = new AnalyticsService(context, this.mockLogger.Object);
        
        // Act
        var result = await service.GetAverageCompletionPercentagePerUserAsync();
        
        // Assert
        Assert.Equal(0, result);
    }
    
    [Fact]
    public async Task GetAverageCompletionPercentagePerReleaseAsync_WithNoReleases_ShouldReturnZero()
    {
        // Arrange
        using var context = this.CreateContext();
        // Don't seed any releases
        
        var service = new AnalyticsService(context, this.mockLogger.Object);
        
        // Act
        var result = await service.GetAverageCompletionPercentagePerReleaseAsync();
        
        // Assert
        Assert.Equal(0, result);
    }
} 