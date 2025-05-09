using Microsoft.EntityFrameworkCore;
using PromotionTasksService.Data;
using PromotionTasksService.Models;

namespace PromotionTasksService.Services;

/// <summary>
/// Service that provides analytics data about promotion tasks.
/// </summary>
public class AnalyticsService
{
    /// <summary>
    /// Gets the database context.
    /// </summary>
    private readonly ApplicationDbContext context;

    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    private readonly ILogger<AnalyticsService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnalyticsService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public AnalyticsService(ApplicationDbContext context, ILogger<AnalyticsService> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    /// <summary>
    /// Gets the overall percentage of completed tasks across all users.
    /// </summary>
    /// <returns>The percentage of completed tasks.</returns>
    public async Task<double> GetOverallCompletionPercentageAsync()
    {
        var allActiveTasks = await this.context.PromotionTasks
            .Where(t => !t.Deleted)
            .ToListAsync();

        if (!allActiveTasks.Any())
        {
            return 0;
        }

        var completedTasks = allActiveTasks.Count(t => t.Status == PromotionTaskStatus.Done);
        return (double)completedTasks / allActiveTasks.Count * 100;
    }

    /// <summary>
    /// Gets the average percentage of completed tasks per user.
    /// </summary>
    /// <returns>A dictionary mapping user IDs to their completion percentage.</returns>
    public async Task<Dictionary<int, double>> GetCompletionPercentagePerUserAsync()
    {
        var users = await this.context.Users
            .Where(u => !u.Deleted)
            .Include(u => u.Releases)
                .ThenInclude(r => r.PromotionTasks.Where(t => !t.Deleted))
            .ToListAsync();

        var userCompletionRates = new Dictionary<int, double>();

        foreach (var user in users)
        {
            var allUserTasks = user.Releases
                .Where(r => !r.Deleted)
                .SelectMany(r => r.PromotionTasks)
                .ToList();

            if (!allUserTasks.Any())
            {
                userCompletionRates[user.UserId] = 0;
                continue;
            }

            var completedTasks = allUserTasks.Count(t => t.Status == PromotionTaskStatus.Done);
            userCompletionRates[user.UserId] = (double)completedTasks / allUserTasks.Count * 100;
        }

        return userCompletionRates;
    }

    /// <summary>
    /// Gets the average completion percentage across all users.
    /// </summary>
    /// <returns>The average completion percentage.</returns>
    public async Task<double> GetAverageCompletionPercentagePerUserAsync()
    {
        var userCompletionRates = await this.GetCompletionPercentagePerUserAsync();
        
        if (!userCompletionRates.Any())
        {
            return 0;
        }

        return userCompletionRates.Values.Average();
    }

    /// <summary>
    /// Gets the completion percentage for each release.
    /// </summary>
    /// <returns>A dictionary mapping release IDs to their completion percentage.</returns>
    public async Task<Dictionary<int, double>> GetCompletionPercentagePerReleaseAsync()
    {
        var releases = await this.context.Releases
            .Where(r => !r.Deleted)
            .Include(r => r.PromotionTasks.Where(t => !t.Deleted))
            .ToListAsync();

        var releaseCompletionRates = new Dictionary<int, double>();

        foreach (var release in releases)
        {
            if (!release.PromotionTasks.Any())
            {
                releaseCompletionRates[release.ReleaseId] = 0;
                continue;
            }

            var completedTasks = release.PromotionTasks.Count(t => t.Status == PromotionTaskStatus.Done);
            releaseCompletionRates[release.ReleaseId] = (double)completedTasks / release.PromotionTasks.Count * 100;
        }

        return releaseCompletionRates;
    }

    /// <summary>
    /// Gets the average completion percentage across all releases.
    /// </summary>
    /// <returns>The average completion percentage.</returns>
    public async Task<double> GetAverageCompletionPercentagePerReleaseAsync()
    {
        var releaseCompletionRates = await this.GetCompletionPercentagePerReleaseAsync();
        
        if (!releaseCompletionRates.Any())
        {
            return 0;
        }

        return releaseCompletionRates.Values.Average();
    }
} 