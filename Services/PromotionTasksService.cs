using Microsoft.EntityFrameworkCore;
using PromotionTasksService.Data;
using PromotionTasksService.Models;

namespace PromotionTasksService.Services;

/// <summary>
/// This service is for managing promotion tasks. These tasks are associated with a release.
/// It uses SQLite for the database and Entity Framework Core for the ORM.
/// </summary>
public class PromotionTasksService
{
    /// <summary>
    /// Gets the database context.
    /// </summary>
    private readonly ApplicationDbContext context;

    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    private readonly ILogger<PromotionTasksService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromotionTasksService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public PromotionTasksService(ApplicationDbContext context, ILogger<PromotionTasksService> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    /// <summary>
    /// Gets all active promotion tasks ordered by priority and due date.
    /// </summary>
    /// <returns>A list of all active promotion tasks.</returns>
    public async Task<List<PromotionTask>> GetPromotionTasksAsync()
    {
        return await this.context.PromotionTasks
            .Where(t => !t.Deleted)
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.DueDate)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a promotion task by its ID.
    /// </summary>
    /// <param name="taskId">The ID of the task to retrieve.</param>
    /// <returns>The task if found and not deleted, null otherwise.</returns>
    public async Task<PromotionTask?> GetPromotionTaskByIdAsync(int taskId)
    {
        return await this.context.PromotionTasks
            .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.Deleted);
    }

    /// <summary>
    /// Creates a new promotion task.
    /// </summary>
    /// <param name="task">The task to create.</param>
    /// <returns>The created task with its assigned ID.</returns>
    public async Task<PromotionTask> CreatePromotionTaskAsync(PromotionTask task)
    {
        task.Deleted = false;
        this.context.PromotionTasks.Add(task);
        await this.context.SaveChangesAsync();
        return task;
    }

    /// <summary>
    /// Updates an existing promotion task.
    /// </summary>
    /// <param name="taskId">The ID of the task to update.</param>
    /// <param name="task">The updated task data.</param>
    /// <returns>The updated task if found, null if the task doesn't exist or is deleted.</returns>
    public async Task<PromotionTask?> UpdatePromotionTaskAsync(int taskId, PromotionTask task)
    {
        var existingTask = await this.GetPromotionTaskByIdAsync(taskId);
        if (existingTask == null)
        {
            return null;
        }

        // Only update fields that are provided (non-default values)
        if (!string.IsNullOrEmpty(task.Description))
        {
            existingTask.Description = task.Description;
        }

        if (task.Status != default)
        {
            existingTask.Status = task.Status;
        }

        if (task.Priority != default)
        {
            existingTask.Priority = task.Priority;
        }

        if (task.DueDate.HasValue)
        {
            existingTask.DueDate = task.DueDate;
        }
        
        existingTask.Deleted = task.Deleted;
        
        this.context.PromotionTasks.Update(existingTask);
        await this.context.SaveChangesAsync();
        return existingTask;
    }

    /// <summary>
    /// Soft deletes a promotion task.
    /// </summary>
    /// <param name="taskId">The ID of the task to delete.</param>
    /// <returns>True if the task was found and deleted, false otherwise.</returns>
    public async Task<bool> DeletePromotionTaskAsync(int taskId)
    {
        var task = await this.GetPromotionTaskByIdAsync(taskId);
        if (task == null)
        {
            return false;
        }

        task.Deleted = true;
        await this.context.SaveChangesAsync();
        return true;
    }
} 