using Microsoft.AspNetCore.Mvc;
using PromotionTasksService.Models.Analytics;
using PromotionTasksService.Services;

namespace PromotionTasksService.Controllers;

/// <summary>
/// Controller for retrieving analytics data about promotion tasks.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    /// <summary>
    /// Gets the analytics service instance.
    /// </summary>
    private readonly AnalyticsService analyticsService;

    /// <summary>
    /// Gets the user service instance.
    /// </summary>
    private readonly UserService userService;

    /// <summary>
    /// Gets the release service instance.
    /// </summary>
    private readonly ReleaseService releaseService;

    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    private readonly ILogger<AnalyticsController> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsController"/> class.
    /// </summary>
    /// <param name="analyticsService">The analytics service.</param>
    /// <param name="userService">The user service.</param>
    /// <param name="releaseService">The release service.</param>
    /// <param name="logger">The logger instance.</param>
    public AnalyticsController(
        AnalyticsService analyticsService,
        UserService userService,
        ReleaseService releaseService,
        ILogger<AnalyticsController> logger)
    {
        this.analyticsService = analyticsService;
        this.userService = userService;
        this.releaseService = releaseService;
        this.logger = logger;
    }

    /// <summary>
    /// Gets overall task completion analytics.
    /// </summary>
    /// <returns>Task completion analytics across all users and releases.</returns>
    [HttpGet]
    [Route("completion")]
    [ProducesResponseType(typeof(TaskCompletionAnalytics), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTaskCompletionAnalytics()
    {
        try
        {
            var overallPercentage = await this.analyticsService.GetOverallCompletionPercentageAsync();
            var avgPerUser = await this.analyticsService.GetAverageCompletionPercentagePerUserAsync();
            var avgPerRelease = await this.analyticsService.GetAverageCompletionPercentagePerReleaseAsync();

            var result = new TaskCompletionAnalytics
            {
                OverallCompletionPercentage = Math.Round(overallPercentage, 2),
                AverageCompletionPercentagePerUser = Math.Round(avgPerUser, 2),
                AverageCompletionPercentagePerRelease = Math.Round(avgPerRelease, 2)
            };

            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving task completion analytics");
            return this.StatusCode(500, "An error occurred while retrieving analytics data");
        }
    }

    /// <summary>
    /// Gets task completion analytics for each user.
    /// </summary>
    /// <returns>A list of user completion analytics.</returns>
    [HttpGet]
    [Route("completion/users")]
    [ProducesResponseType(typeof(List<UserCompletionAnalytics>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserCompletionAnalytics()
    {
        try
        {
            var users = await this.userService.GetAllUsersAsync();
            var percentagesPerUser = await this.analyticsService.GetCompletionPercentagePerUserAsync();

            var result = new List<UserCompletionAnalytics>();

            foreach (var user in users)
            {
                if (percentagesPerUser.TryGetValue(user.UserId, out var percentage))
                {
                    result.Add(new UserCompletionAnalytics
                    {
                        UserId = user.UserId,
                        UserName = user.Name,
                        CompletionPercentage = Math.Round(percentage, 2)
                    });
                }
            }

            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving user completion analytics");
            return this.StatusCode(500, "An error occurred while retrieving user analytics data");
        }
    }

    /// <summary>
    /// Gets task completion analytics for each release.
    /// </summary>
    /// <returns>A list of release completion analytics.</returns>
    [HttpGet]
    [Route("completion/releases")]
    [ProducesResponseType(typeof(List<ReleaseCompletionAnalytics>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReleaseCompletionAnalytics()
    {
        try
        {
            var releases = await this.releaseService.GetAllReleasesAsync();
            var percentagesPerRelease = await this.analyticsService.GetCompletionPercentagePerReleaseAsync();

            var result = new List<ReleaseCompletionAnalytics>();

            foreach (var release in releases)
            {
                if (percentagesPerRelease.TryGetValue(release.ReleaseId, out var percentage))
                {
                    var totalTasks = release.PromotionTasks?.Count(t => !t.Deleted) ?? 0;
                    var completedTasks = release.PromotionTasks?.Count(t => !t.Deleted && t.Status == Models.PromotionTaskStatus.Done) ?? 0;

                    result.Add(new ReleaseCompletionAnalytics
                    {
                        ReleaseId = release.ReleaseId,
                        ReleaseTitle = release.Title,
                        UserId = release.UserId,
                        CompletionPercentage = Math.Round(percentage, 2),
                        TotalTasks = totalTasks,
                        CompletedTasks = completedTasks
                    });
                }
            }

            return this.Ok(result);
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error retrieving release completion analytics");
            return this.StatusCode(500, "An error occurred while retrieving release analytics data");
        }
    }
} 