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

public class TasksServiceTests
{
    private readonly DbContextOptions<ApplicationDbContext> dbContextOptions;
    private readonly Mock<ILogger<PromotionTasksService.Services.PromotionTasksService>> mockLogger;
    
    public TasksServiceTests()
    {
        this.dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
            
        this.mockLogger = new Mock<ILogger<PromotionTasksService.Services.PromotionTasksService>>();
    }
    
    private ApplicationDbContext CreateContext()
    {
        var context = new ApplicationDbContext(this.dbContextOptions);
        context.Database.EnsureCreated();
        return context;
    }
    
    [Fact]
    public async Task GetPromotionTaskByIdAsync_WhenTaskExists_ShouldReturnTask()
    {
        // Arrange
        using var context = this.CreateContext();
        
        var testTask = new PromotionTask
        {
            TaskId = 1,
            ReleaseId = 1,
            Description = "Test Task",
            Status = PromotionTaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(1),
            Deleted = false
        };
        
        context.PromotionTasks.Add(testTask);
        context.SaveChanges();
        
        var service = new PromotionTasksService.Services.PromotionTasksService(context, this.mockLogger.Object);
        
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
        using var context = this.CreateContext();
        
        var testTask = new PromotionTask
        {
            TaskId = 1,
            ReleaseId = 1,
            Description = "Test Task",
            Status = PromotionTaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(1),
            Deleted = true
        };
        
        context.PromotionTasks.Add(testTask);
        context.SaveChanges();
        
        var service = new PromotionTasksService.Services.PromotionTasksService(context, this.mockLogger.Object);
        
        // Act
        var result = await service.GetPromotionTaskByIdAsync(1);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task GetPromotionTasksAsync_ShouldReturnAllNonDeletedTasks()
    {
        // Arrange
        using var context = this.CreateContext();
        
        var now = DateTime.UtcNow;
        context.PromotionTasks.AddRange(
            new PromotionTask { TaskId = 1, ReleaseId = 1, Description = "Task 1", Status = PromotionTaskStatus.ToDo, Priority = TaskPriority.Low, DueDate = now, Deleted = false },
            new PromotionTask { TaskId = 2, ReleaseId = 1, Description = "Task 2", Status = PromotionTaskStatus.InProgress, Priority = TaskPriority.Medium, DueDate = now, Deleted = false },
            new PromotionTask { TaskId = 3, ReleaseId = 1, Description = "Task 3", Status = PromotionTaskStatus.Done, Priority = TaskPriority.High, DueDate = now, Deleted = true },
            new PromotionTask { TaskId = 4, ReleaseId = 2, Description = "Task 4", Status = PromotionTaskStatus.ToDo, Priority = TaskPriority.Urgent, DueDate = now, Deleted = false }
        );
        context.SaveChanges();
        
        var service = new PromotionTasksService.Services.PromotionTasksService(context, this.mockLogger.Object);
        
        // Act
        var result = await service.GetPromotionTasksAsync();
        
        // Assert
        Assert.Equal(3, result.Count);
        Assert.DoesNotContain(result, t => t.Deleted);
    }
    
    [Fact]
    public async Task CreatePromotionTaskAsync_ShouldAddNewTask()
    {
        // Arrange
        using var context = this.CreateContext();
        var service = new PromotionTasksService.Services.PromotionTasksService(context, this.mockLogger.Object);
        
        var newTask = new PromotionTask
        {
            ReleaseId = 1,
            Description = "New Task",
            Status = PromotionTaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow.AddDays(1)
        };
        
        // Act
        var result = await service.CreatePromotionTaskAsync(newTask);
        
        // Assert
        Assert.NotEqual(0, result.TaskId);
        Assert.False(result.Deleted);
        
        // Verify it's in the database
        var taskInDb = await context.PromotionTasks.FindAsync(result.TaskId);
        Assert.NotNull(taskInDb);
        Assert.Equal("New Task", taskInDb.Description);
    }
    
    [Fact]
    public async Task UpdatePromotionTaskAsync_WhenTaskExists_ShouldUpdateTask()
    {
        // Arrange
        using var context = this.CreateContext();
        
        var existingTask = new PromotionTask
        {
            TaskId = 1,
            ReleaseId = 1,
            Description = "Original Task",
            Status = PromotionTaskStatus.ToDo,
            Priority = TaskPriority.Low,
            DueDate = DateTime.UtcNow,
            Deleted = false
        };
        
        context.PromotionTasks.Add(existingTask);
        context.SaveChanges();
        
        var service = new PromotionTasksService.Services.PromotionTasksService(context, this.mockLogger.Object);
        
        // Create a new task object with only the properties we want to update
        var updatedTask = new PromotionTask
        {
            Description = "Updated Task",
            Status = PromotionTaskStatus.InProgress,
            Priority = TaskPriority.High,
            DueDate = DateTime.UtcNow.AddDays(1)
        };
        
        // Act
        var result = await service.UpdatePromotionTaskAsync(1, updatedTask);
        
        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TaskId);  // ID should remain unchanged
        Assert.Equal("Updated Task", result.Description);
        Assert.Equal(PromotionTaskStatus.InProgress, result.Status);
        Assert.Equal(TaskPriority.High, result.Priority);
        Assert.False(result.Deleted);  // Deleted status should remain unchanged
    }
    
    [Fact]
    public async Task DeletePromotionTaskAsync_WhenTaskExists_ShouldReturnTrue()
    {
        // Arrange
        using var context = this.CreateContext();
        
        var testTask = new PromotionTask
        {
            TaskId = 1,
            ReleaseId = 1,
            Description = "Test Task",
            Status = PromotionTaskStatus.ToDo,
            Priority = TaskPriority.Medium,
            DueDate = DateTime.UtcNow,
            Deleted = false
        };
        
        context.PromotionTasks.Add(testTask);
        context.SaveChanges();
        
        var service = new PromotionTasksService.Services.PromotionTasksService(context, this.mockLogger.Object);
        
        // Act
        var result = await service.DeletePromotionTaskAsync(1);
        
        // Assert
        Assert.True(result);
        
        // Verify it's marked as deleted in the database
        var taskInDb = await context.PromotionTasks.FindAsync(1);
        Assert.NotNull(taskInDb);
        Assert.True(taskInDb.Deleted);
    }
} 