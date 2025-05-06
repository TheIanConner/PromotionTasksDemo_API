namespace PromotionTasksService.Models;

/// <summary>
/// Defines the possible statuses for a promotion task.
/// </summary>
public enum PromotionTaskStatus
{
    /// <summary>
    /// Task has not been started yet.
    /// </summary>
    ToDo,

    /// <summary>
    /// Task is currently being worked on.
    /// </summary>
    InProgress,

    /// <summary>
    /// Task has been completed.
    /// </summary>
    Done,
} 