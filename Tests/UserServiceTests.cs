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

public class UserServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> dbContextOptions;
    private readonly Mock<ILogger<UserService>> mockUserLogger;
    
    public UserServiceTests()
    {
        this.dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        this.mockUserLogger = new Mock<ILogger<UserService>>();
    }
    
    private ApplicationDbContext CreateContext()
    {
        var context = new ApplicationDbContext(this.dbContextOptions);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetAllUsersAsync_ShouldReturnAllNonDeletedUsers()
    {
        // Arrange
        using var context = this.CreateContext();
        
        context.Users.AddRange(
            new User { UserId = 1, Name = "User 1", Deleted = false },
            new User { UserId = 2, Name = "User 2", Deleted = false },
            new User { UserId = 3, Name = "User 3", Deleted = true }
        );
        context.SaveChanges();
        
        var service = new UserService(context, this.mockUserLogger.Object);
        
        // Act
        var result = await service.GetAllUsersAsync();
        
        // Assert
        Assert.Equal(2, result.Count());
        Assert.DoesNotContain(result, u => u.Deleted);
        Assert.Equal("User 1", result.First().Name); // Ordered by name
    }

    [Fact]
    public async Task GetUserByIdWithReleasesAndTasksAsync_WhenUserExists_ShouldReturnUserWithReleasesAndTasks()
    {
        // Arrange
        using var context = this.CreateContext();
        
        var userId = 1;
        var user = new User { UserId = userId, Name = "Test User", Deleted = false };
        var release = new Release { ReleaseId = 1, Title = "Test Release", UserId = userId, Type = ReleaseType.Single, ReleaseDate = DateTime.UtcNow, Deleted = false };
        var task = new PromotionTask { TaskId = 1, Description = "Test Task", ReleaseId = 1, Deleted = false };
        
        context.Users.Add(user);
        context.Releases.Add(release);
        context.PromotionTasks.Add(task);
        context.SaveChanges();
        
        var service = new UserService(context, this.mockUserLogger.Object);
        
        // Act
        var result = await service.GetUserByIdWithReleasesAndTasksAsync(userId);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Single(result.Releases);
        Assert.Single(result.Releases.First().PromotionTasks);
    }

    [Fact]
    public async Task GetUserByIdWithReleasesAndTasksAsync_WhenUserDeleted_ShouldReturnNull()
    {
        // Arrange
        using var context = this.CreateContext();
        
        var userId = 1;
        var user = new User { UserId = userId, Name = "Test User", Deleted = true };
        
        context.Users.Add(user);
        context.SaveChanges();
        
        var service = new UserService(context, this.mockUserLogger.Object);
        
        // Act
        var result = await service.GetUserByIdWithReleasesAndTasksAsync(userId);
        
        // Assert
        Assert.Null(result);
    }
} 