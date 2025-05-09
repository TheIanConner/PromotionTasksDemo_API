namespace PromotionTasksService.Models.Analytics;

/// <summary>
/// Represents analytics data for task completion.
/// </summary>
public class TaskCompletionAnalytics
{
    /// <summary>
    /// Gets or sets the overall percentage of completed tasks across all users.
    /// </summary>
    public double OverallCompletionPercentage { get; set; }

    /// <summary>
    /// Gets or sets the average completion percentage per user.
    /// </summary>
    public double AverageCompletionPercentagePerUser { get; set; }

    /// <summary>
    /// Gets or sets the average completion percentage per release.
    /// </summary>
    public double AverageCompletionPercentagePerRelease { get; set; }
} 