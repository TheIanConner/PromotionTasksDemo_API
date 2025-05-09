namespace PromotionTasksService.Models.Analytics;

/// <summary>
/// Represents task completion analytics for a specific user.
/// </summary>
public class UserCompletionAnalytics
{
    /// <summary>
    /// Gets or sets the user ID.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the completion percentage for this user's tasks.
    /// </summary>
    public double CompletionPercentage { get; set; }
} 