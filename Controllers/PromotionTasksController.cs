using Microsoft.AspNetCore.Mvc;
using PromotionTasksService.Models;
using PromotionTasksService.Services;

namespace PromotionTasksService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PromotionTasksController : ControllerBase
{
    private readonly Services.PromotionTasksService _taskService;
    private readonly ILogger<PromotionTasksController> _logger;

    public PromotionTasksController(Services.PromotionTasksService taskService, ILogger<PromotionTasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PromotionTask>>> GetAllTasks()
    {
        try
        {
            var tasks = await _taskService.GetPromotionTasksAsync();
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tasks");
            return StatusCode(500, "An error occurred while retrieving tasks");
        }
    }

    [HttpGet("{taskId}")]
    public async Task<ActionResult<PromotionTask>> GetTaskById(int taskId)
    {
        try
        {
            var task = await _taskService.GetPromotionTaskByIdAsync(taskId);
            if (task == null)
            {
                return NotFound($"Task with ID {taskId} not found");
            }
            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task with ID {TaskId}", taskId);
            return StatusCode(500, "An error occurred while retrieving the task");
        }
    }

    [HttpPut("{taskId}/status")]
    public async Task<ActionResult<PromotionTask>> UpdateTaskStatus(int taskId, [FromBody] PromotionTaskStatus newStatus)
    {
        try
        {
            var task = new PromotionTask { Status = newStatus };
            var updatedTask = await _taskService.UpdatePromotionTaskAsync(taskId, task);
            if (updatedTask == null)
            {
                return NotFound($"Task with ID {taskId} not found");
            }
            return Ok(updatedTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task status for task ID {TaskId}", taskId);
            return StatusCode(500, "An error occurred while updating the task status");
        }
    }

    [HttpPost]
    public async Task<ActionResult<PromotionTask>> CreateOrUpdateTask([FromBody] PromotionTask task)
    {
        try
        {
            PromotionTask result;
            if (task.TaskId == 0)
            {
                result = await _taskService.CreatePromotionTaskAsync(task);
            }
            else
            {
                result = await _taskService.UpdatePromotionTaskAsync(task.TaskId, task) ?? 
                    throw new Exception("Failed to update task");
            }
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating/updating task");
            return StatusCode(500, "An error occurred while saving the task");
        }
    }
} 