namespace PromotionTasksService.Models.Analytics;

/// <summary>
/// Represents task completion analytics for a specific release.
/// </summary>
public class ReleaseCompletionAnalytics
{
    /// <summary>
    /// Gets or sets the release ID.
    /// </summary>
    public int ReleaseId { get; set; }

    /// <summary>
    /// Gets or sets the release title.
    /// </summary>
    public string ReleaseTitle { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the user ID of the release owner.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the completion percentage for this release's tasks.
    /// </summary>
    public double CompletionPercentage { get; set; }

    /// <summary>
    /// Gets or sets the number of total tasks for this release.
    /// </summary>
    public int TotalTasks { get; set; }

    /// <summary>
    /// Gets or sets the number of completed tasks for this release.
    /// </summary>
    public int CompletedTasks { get; set; }
} 