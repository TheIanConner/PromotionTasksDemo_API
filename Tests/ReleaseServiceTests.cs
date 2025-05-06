using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PromotionTasksService.Data;
using PromotionTasksService.Models;
using PromotionTasksService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PromotionTasksService.Tests.Services;

public class ReleaseServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly Mock<ILogger<ReleaseService>> _mockReleaseLogger;
    
    public ReleaseServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _mockReleaseLogger = new Mock<ILogger<ReleaseService>>();
    }
    
    private ApplicationDbContext CreateContext()
    {
        var context = new ApplicationDbContext(_dbContextOptions);
        context.Database.EnsureCreated();
        return context;
    }

    [Fact]
    public async Task GetReleaseByIdAsync_WhenReleaseExists_ShouldReturnRelease()
    {
        // Arrange
        using var context = CreateContext();
        
        var testRelease = new Release
        {
            ReleaseId = 1,
            Title = "Test Release",
            Description = "Test Description",
            UserId = 1,
            Type = ReleaseType.Single,
            ReleaseDate = DateTime.UtcNow,
            Deleted = false
        };
        
        context.Releases.Add(testRelease);
        context.SaveChanges();
        
        var service = new ReleaseService(context, _mockReleaseLogger.Object);
        
        // Act
        var result = await service.GetReleaseByIdAsync(1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ReleaseId);
        Assert.Equal("Test Release", result.Title);
    }

    [Fact]
    public async Task GetReleaseByIdAsync_WhenReleaseDeleted_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        
        var testRelease = new Release
        {
            ReleaseId = 1,
            Title = "Test Release",
            Description = "Test Description",
            UserId = 1,
            Type = ReleaseType.Single,
            ReleaseDate = DateTime.UtcNow,
            Deleted = true
        };
        
        context.Releases.Add(testRelease);
        context.SaveChanges();
        
        var service = new ReleaseService(context, _mockReleaseLogger.Object);
        
        // Act
        var result = await service.GetReleaseByIdAsync(1);
        
        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetUserReleasesAsync_ShouldReturnAllNonDeletedReleasesForUser()
    {
        // Arrange
        using var context = CreateContext();
        
        var userId = 1;
        var now = DateTime.UtcNow;
        context.Releases.AddRange(
            new Release { ReleaseId = 1, Title = "Release 1", Description = "Description 1", UserId = userId, Type = ReleaseType.Single, ReleaseDate = now, Deleted = false },
            new Release { ReleaseId = 2, Title = "Release 2", Description = "Description 2", UserId = userId, Type = ReleaseType.EP, ReleaseDate = now, Deleted = false },
            new Release { ReleaseId = 3, Title = "Release 3", Description = "Description 3", UserId = userId, Type = ReleaseType.Album, ReleaseDate = now, Deleted = true },
            new Release { ReleaseId = 4, Title = "Release 4", Description = "Description 4", UserId = 2, Type = ReleaseType.Mixtape, ReleaseDate = now, Deleted = false }
        );
        context.SaveChanges();
        
        var service = new ReleaseService(context, _mockReleaseLogger.Object);
        
        // Act
        var result = await service.GetUserReleasesAsync(userId);
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, r => r.Deleted);
        Assert.DoesNotContain(result, r => r.UserId != userId);
    }

    [Fact]
    public async Task CreateReleaseAsync_ShouldAddNewRelease()
    {
        // Arrange
        using var context = CreateContext();
        var service = new ReleaseService(context, _mockReleaseLogger.Object);
        
        var newRelease = new Release
        {
            Title = "New Release",
            Description = "New Description",
            UserId = 1,
            Type = ReleaseType.Single,
            ReleaseDate = DateTime.UtcNow
        };
        
        // Act
        var result = await service.CreateReleaseAsync(newRelease);
        
        // Assert
        Assert.NotEqual(0, result.ReleaseId);
        Assert.False(result.Deleted);
        
        // Verify it's in the database
        var releaseInDb = await context.Releases.FindAsync(result.ReleaseId);
        Assert.NotNull(releaseInDb);
        Assert.Equal("New Release", releaseInDb.Title);
    }

    [Fact]
    public async Task UpdateReleaseAsync_WhenReleaseExists_ShouldUpdateRelease()
    {
        // Arrange
        using var context = CreateContext();
        
        var existingRelease = new Release
        {
            ReleaseId = 1,
            Title = "Original Title",
            Description = "Original Description",
            UserId = 1,
            Type = ReleaseType.Single,
            ReleaseDate = DateTime.UtcNow,
            Deleted = false
        };
        
        context.Releases.Add(existingRelease);
        context.SaveChanges();
        
        var service = new ReleaseService(context, _mockReleaseLogger.Object);
        
        // Create a new release object with only the properties we want to update
        var updatedRelease = new Release
        {
            Title = "Updated Title",
            Description = "Updated Description",
            UserId = 2,
            Type = ReleaseType.Album,
            ReleaseDate = DateTime.UtcNow.AddDays(1),
            Deleted = false
        };
        
        // Act
        var result = await service.UpdateReleaseAsync(1, updatedRelease);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.ReleaseId);  // ID should remain unchanged
        Assert.Equal("Updated Title", result.Title);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(2, result.UserId);
        Assert.Equal(ReleaseType.Album, result.Type);
        Assert.False(result.Deleted);  // Deleted status should remain unchanged
    }

    [Fact]
    public async Task DeleteReleaseAsync_WhenReleaseExists_ShouldMarkAsDeleted()
    {
        // Arrange
        using var context = CreateContext();
        
        var existingRelease = new Release
        {
            ReleaseId = 1,
            Title = "Release to Delete",
            Description = "Description to Delete",
            UserId = 1,
            Type = ReleaseType.Single,
            ReleaseDate = DateTime.UtcNow,
            Deleted = false
        };
        
        context.Releases.Add(existingRelease);
        context.SaveChanges();
        
        var service = new ReleaseService(context, _mockReleaseLogger.Object);
        
        // Act
        var result = await service.DeleteReleaseAsync(1);
        
        // Assert
        Assert.True(result);
        
        var releaseInDb = await context.Releases.FindAsync(1);
        Assert.NotNull(releaseInDb);
        Assert.True(releaseInDb!.Deleted);
    }
} 