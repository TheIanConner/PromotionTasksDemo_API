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
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PromotionTasksService> _logger;

    public PromotionTasksService(ApplicationDbContext context, ILogger<PromotionTasksService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<PromotionTask>> GetPromotionTasksAsync()
    {
        return await _context.PromotionTasks
            .Where(t => !t.Deleted)
            .OrderByDescending(t => t.Priority)
            .ThenByDescending(t => t.DueDate)
            .ToListAsync();
    }

    public async Task<PromotionTask?> GetPromotionTaskByIdAsync(int taskId)
    {
        return await _context.PromotionTasks
            .FirstOrDefaultAsync(t => t.TaskId == taskId && !t.Deleted);
    }

    public async Task<PromotionTask> CreatePromotionTaskAsync(PromotionTask task)
    {
        task.Deleted = false;
        _context.PromotionTasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<PromotionTask?> UpdatePromotionTaskAsync(int taskId, PromotionTask task)
    {
        var existingTask = await GetPromotionTaskByIdAsync(taskId);
        if (existingTask == null)
        {
            return null;
        }

        existingTask.Description = task.Description;
        existingTask.Status = task.Status;
        existingTask.Priority = task.Priority;
        existingTask.DueDate = task.DueDate;
        existingTask.ReleaseId = task.ReleaseId;
        
        _context.PromotionTasks.Update(existingTask);
        await _context.SaveChangesAsync();
        return existingTask;
    }

    public async Task<bool> DeletePromotionTaskAsync(int taskId)
    {
        var task = await GetPromotionTaskByIdAsync(taskId);
        if (task == null)
        {
            return false;
        }

        task.Deleted = true;
        await _context.SaveChangesAsync();
        return true;
    }
} 