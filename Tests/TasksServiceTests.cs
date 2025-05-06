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

public class PromotionTasksServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> _dbContextOptions;
    private readonly Mock<ILogger<PromotionTasksService.Services.PromotionTasksService>> _mockLogger;
    
    public PromotionTasksServiceTests()
    {
        _dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        _mockLogger = new Mock<ILogger<PromotionTasksService.Services.PromotionTasksService>>();
    }
    
    private ApplicationDbContext CreateContext()
    {
        var context = new ApplicationDbContext(_dbContextOptions);
        context.Database.EnsureCreated();
        return context;
    }
    
    [Fact]
    public async Task GetPromotionTasksAsync_ShouldReturnAllNonDeletedTasks()
    {
        // Arrange
        using var context = CreateContext();
        
        // Add test data
        context.PromotionTasks.AddRange(
            new PromotionTask
            {
                TaskId = 1,
                Description = "Task 1",
                Status = PromotionTaskStatus.ToDo,
                Priority = TaskPriority.High,
                ReleaseId = 1,
                Deleted = false
            },
            new PromotionTask
            {
                TaskId = 2,
                Description = "Task 2",
                Status = PromotionTaskStatus.InProgress,
                Priority = TaskPriority.Medium,
                ReleaseId = 1,
                Deleted = false
            },
            new PromotionTask
            {
                TaskId = 3,
                Description = "Task 3",
                Status = PromotionTaskStatus.Done,
                Priority = TaskPriority.Low,
                ReleaseId = 1,
                Deleted = true // Should not be returned
            }
        );
        context.SaveChanges();
        
        var service = new PromotionTasksService.Services.PromotionTasksService(context, _mockLogger.Object);
        
        // Act
        var result = await service.GetPromotionTasksAsync();
        
        // Assert
        Assert.Equal(2, result.Count);
        Assert.DoesNotContain(result, t => t.Deleted);
        Assert.Equal(TaskPriority.High, result[0].Priority); // Ordered by priority descending
    }
    
    [Fact]
    public async Task GetPromotionTaskByIdAsync_WhenTaskExists_ShouldReturnTask()
    {
        // Arrange
        using var context = CreateContext();
        
        var testTask = new PromotionTask
        {
            TaskId = 1,
            Description = "Test Task",
            Status = PromotionTaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            ReleaseId = 1,
            Deleted = false
        };
        
        context.PromotionTasks.Add(testTask);
        context.SaveChanges();
        
        var service = new PromotionTasksService.Services.PromotionTasksService(context, _mockLogger.Object);
        
        // Act
        var result = await service.GetPromotionTaskByIdAsync(1);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TaskId);
        Assert.Equal("Test Task", result.Description);
    }
    
    [Fact]
    public async Task GetPromotionTaskByIdAsync_WhenTaskDeleted_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        
        var testTask = new PromotionTask
        {
            TaskId = 1,
            Description = "Test Task",
            Status = PromotionTaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            ReleaseId = 1,
            Deleted = true // Deleted task should not be returned
        };
        
        context.PromotionTasks.Add(testTask);
        context.SaveChanges();
        
        var service = new PromotionTasksService.Services.PromotionTasksService(context, _mockLogger.Object);
        
        // Act
        var result = await service.GetPromotionTaskByIdAsync(1);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task CreatePromotionTaskAsync_ShouldAddNewTask()
    {
        // Arrange
        using var context = CreateContext();
        var service = new PromotionTasksService.Services.PromotionTasksService(context, _mockLogger.Object);
        
        var newTask = new PromotionTask
        {
            Description = "New Task",
            Status = PromotionTaskStatus.ToDo,
            Priority = TaskPriority.High,
            ReleaseId = 1
        };
        
        // Act
        var result = await service.CreatePromotionTaskAsync(newTask);
        
        // Assert
        Assert.NotEqual(0, result.TaskId); // Should have an ID assigned
        Assert.False(result.Deleted); // Should not be marked as deleted
        
        // Verify it's in the database
        var taskInDb = await context.PromotionTasks.FindAsync(result.TaskId);
        Assert.NotNull(taskInDb);
        Assert.Equal("New Task", taskInDb.Description);
    }
    
    [Fact]
    public async Task UpdatePromotionTaskAsync_WhenTaskExists_ShouldUpdateTask()
    {
        // Arrange
        using var context = CreateContext();
        
        var existingTask = new PromotionTask
        {
            TaskId = 1,
            Description = "Original Description",
            Status = PromotionTaskStatus.ToDo,
            Priority = TaskPriority.Low,
            ReleaseId = 1,
            Deleted = false
        };
        
        context.PromotionTasks.Add(existingTask);
        context.SaveChanges();
        
        var service = new PromotionTasksService.Services.PromotionTasksService(context, _mockLogger.Object);
        
        var updatedTask = new PromotionTask
        {
            Description = "Updated Description",
            Status = PromotionTaskStatus.InProgress,
            Priority = TaskPriority.High,
            ReleaseId = 2
        };
        
        // Act
        var result = await service.UpdatePromotionTaskAsync(1, updatedTask);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TaskId);
        Assert.Equal("Updated Description", result.Description);
        Assert.Equal(PromotionTaskStatus.InProgress, result.Status);
        Assert.Equal(TaskPriority.High, result.Priority);
        Assert.Equal(2, result.ReleaseId);
        
        // Verify changes are in the database
        context.Entry(result).State = EntityState.Detached;
        var taskInDb = await context.PromotionTasks.FindAsync(1);
        Assert.NotNull(taskInDb);
        Assert.Equal("Updated Description", taskInDb!.Description);
    }
    
    [Fact]
    public async Task UpdatePromotionTaskAsync_WhenTaskDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        using var context = CreateContext();
        var service = new PromotionTasksService.Services.PromotionTasksService(context, _mockLogger.Object);
        
        var updatedTask = new PromotionTask
        {
            Description = "Updated Description",
            Status = PromotionTaskStatus.InProgress,
            Priority = TaskPriority.High,
            ReleaseId = 2
        };
        
        // Act
        var result = await service.UpdatePromotionTaskAsync(999, updatedTask); // Non-existent ID
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task DeletePromotionTaskAsync_WhenTaskExists_ShouldMarkAsDeleted()
    {
        // Arrange
        using var context = CreateContext();
        
        var existingTask = new PromotionTask
        {
            TaskId = 1,
            Description = "Task to Delete",
            Status = PromotionTaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            ReleaseId = 1,
            Deleted = false
        };
        
        context.PromotionTasks.Add(existingTask);
        context.SaveChanges();
        
        var service = new PromotionTasksService.Services.PromotionTasksService(context, _mockLogger.Object);
        
        // Act
        var result = await service.DeletePromotionTaskAsync(1);
        
        // Assert
        Assert.True(result);
        
        // Verify it's marked as deleted in the database
        var taskInDb = await context.PromotionTasks.FindAsync(1);
        Assert.NotNull(taskInDb);
        Assert.True(taskInDb.Deleted);
        
        // Verify it doesn't show up in queries
        var deletedTask = await service.GetPromotionTaskByIdAsync(1);
        Assert.Null(deletedTask);
    }
    
    [Fact]
    public async Task DeletePromotionTaskAsync_WhenTaskDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateContext();
        var service = new PromotionTasksService.Services.PromotionTasksService(context, _mockLogger.Object);
        
        // Act
        var result = await service.DeletePromotionTaskAsync(999); // Non-existent ID
        
        // Assert
        Assert.False(result);
    }
} 