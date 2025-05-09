using Microsoft.AspNetCore.Mvc;
using PromotionTasksService.Models;
using PromotionTasksService.Services;

namespace PromotionTasksService.Controllers;

/// <summary>
/// Controller for managing promotion tasks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PromotionTasksController : ControllerBase
{
    /// <summary>
    /// Gets the promotion tasks service.
    /// </summary>
    private readonly Services.PromotionTasksService taskService;

    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    private readonly ILogger<PromotionTasksController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PromotionTasksController"/> class.
    /// </summary>
    /// <param name="taskService">The promotion tasks service.</param>
    /// <param name="logger">The logger.</param>
    public PromotionTasksController(Services.PromotionTasksService taskService, ILogger<PromotionTasksController> logger)
    {
        this.taskService = taskService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets all non-deleted promotion tasks.
    /// </summary>
    /// <returns>A list of all active (non-deleted) promotion tasks.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PromotionTask>>> GetAllTasks()
    {
        try
        {
            var tasks = await this.taskService.GetPromotionTasksAsync();
            return this.Ok(tasks);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting all tasks");
            return this.StatusCode(500, "An error occurred while retrieving tasks");
        }
    }

    /// <summary>
    /// Gets a promotion task by its ID.
    /// </summary>
    /// <param name="taskId">The ID of the task to retrieve.</param>
    /// <returns>The promotion task if found, or NotFound if not found.</returns>
    [HttpGet("{taskId}")]
    public async Task<ActionResult<PromotionTask>> GetTaskById(int taskId)
    {
        try
        {
            var task = await this.taskService.GetPromotionTaskByIdAsync(taskId);
            if (task == null)
            {
                return this.NotFound($"Task with ID {taskId} not found");
            }

            return this.Ok(task);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error getting task with ID {TaskId}", taskId);
            return this.StatusCode(500, "An error occurred while retrieving the task");
        }
    }

    /// <summary>
    /// Updates the status of a promotion task.
    /// </summary>
    /// <param name="taskId">The ID of the task to update.</param>
    /// <param name="newStatus">The new status for the task.</param>
    /// <returns>The updated promotion task if successful, or NotFound if the task doesn't exist.</returns>
    [HttpPut("{taskId}/status")]
    public async Task<ActionResult<PromotionTask>> UpdateTaskStatus(int taskId, [FromBody] PromotionTaskStatus newStatus)
    {
        try
        {
            var task = await this.taskService.GetPromotionTaskByIdAsync(taskId);
            if (task == null)
            {
                return this.NotFound($"Task with ID {taskId} not found");
            }

            // Update the status
            task.Status = newStatus;

            var updatedTask = await this.taskService.UpdatePromotionTaskAsync(taskId, task);
            if (updatedTask == null)
            {
                return this.NotFound($"Task with ID {taskId} not found");
            }

            return this.Ok(updatedTask);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating task status for task ID {TaskId}", taskId);
            return this.StatusCode(500, "An error occurred while updating the task status");
        }
    }

    /// <summary>
    /// Updates the priority of a promotion task.
    /// </summary>
    /// <param name="taskId">The ID of the task to update.</param>
    /// <param name="newPriority">The new priority for the task.</param>
    /// <returns>The updated promotion task if successful, or NotFound if the task doesn't exist.</returns>
    [HttpPut("{taskId}/priority")]
    public async Task<ActionResult<PromotionTask>> UpdateTaskPriority(int taskId, [FromBody] TaskPriority newPriority)
    {
        try
        {
            var task = await this.taskService.GetPromotionTaskByIdAsync(taskId);
            if (task == null)
            {
                return this.NotFound($"Task with ID {taskId} not found");
            }

            // Update the priority
            task.Priority = newPriority;

            var updatedTask = await this.taskService.UpdatePromotionTaskAsync(taskId, task);
            if (updatedTask == null)
            {
                return this.NotFound($"Task with ID {taskId} not found");
            }

            return this.Ok(updatedTask);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error updating task priority for task ID {TaskId}", taskId);
            return this.StatusCode(500, "An error occurred while updating the task priority");
        }
    }

    /// <summary>
    /// Creates a new promotion task or updates an existing one.
    /// </summary>
    /// <param name="task">The promotion task to create or update.</param>
    /// <returns>The created or updated promotion task.</returns>
    [HttpPost]
    public async Task<ActionResult<PromotionTask>> CreateOrUpdateTask([FromBody] PromotionTask task)
    {
        try
        {
            PromotionTask result;
            if (task.TaskId == 0)
            {
                result = await this.taskService.CreatePromotionTaskAsync(task);
            }
            else
            {
                result = await this.taskService.UpdatePromotionTaskAsync(task.TaskId, task) ?? 
                    throw new Exception("Failed to update task");
            }

            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error creating/updating task");
            return this.StatusCode(500, "An error occurred while saving the task");
        }
    }
} 