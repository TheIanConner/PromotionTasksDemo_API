namespace PromotionTasksService.Models;

/// <summary>
/// Defines the priority levels available for tasks.
/// </summary>
public enum TaskPriority
{
    /// <summary>
    /// Urgent tasks that require immediate attention.
    /// </summary>
    Urgent = 0,

    /// <summary>
    /// High priority tasks that need attention soon.
    /// </summary>
    High,

    /// <summary>
    /// Medium priority tasks that should be completed in a reasonable timeframe.
    /// </summary>
    Medium,

    /// <summary>
    /// Low priority tasks that are not time-critical.
    /// </summary>
    Low,
} 