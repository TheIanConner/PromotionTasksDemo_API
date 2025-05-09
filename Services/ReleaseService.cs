using Microsoft.EntityFrameworkCore;
using PromotionTasksService.Data;
using PromotionTasksService.Models;

namespace PromotionTasksService.Services;

/// <summary>
/// This service is for managing releases for a user.
/// It uses SQLite for the database and Entity Framework Core for the ORM.
/// </summary>
public class ReleaseService
{
    /// <summary>
    /// Gets the database context.
    /// </summary>
    private readonly ApplicationDbContext context;

    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    private readonly ILogger<ReleaseService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReleaseService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public ReleaseService(ApplicationDbContext context, ILogger<ReleaseService> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    /// <summary>
    /// Gets all active releases.
    /// </summary>
    /// <returns>A list of all active releases.</returns>
    public async Task<IEnumerable<Release>> GetAllReleasesAsync()
    {
        return await this.context.Releases
            .Include(r => r.PromotionTasks)
            .Where(r => !r.Deleted)
            .ToListAsync();
    }

    /// <summary>
    /// Gets a release by its ID.
    /// </summary>
    /// <param name="id">The ID of the release to retrieve.</param>
    /// <returns>The release if found and not deleted, null otherwise.</returns>
    public async Task<Release?> GetReleaseByIdAsync(int id)
    {
        return await this.context.Releases
            .FirstOrDefaultAsync(r => r.ReleaseId == id && !r.Deleted);
    }

    /// <summary>
    /// Gets all releases for a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user whose releases to retrieve.</param>
    /// <returns>A list of all active releases for the user.</returns>
    public async Task<List<Release>> GetUserReleasesAsync(int userId)
    {
        return await this.context.Releases
            .Where(r => r.UserId == userId && !r.Deleted)
            .ToListAsync();
    }

    /// <summary>
    /// Creates a new release.
    /// </summary>
    /// <param name="release">The release to create.</param>
    /// <returns>The created release with its assigned ID.</returns>
    public async Task<Release> CreateReleaseAsync(Release release)
    {
        this.context.Releases.Add(release);
        await this.context.SaveChangesAsync();
        return release;
    }

    /// <summary>
    /// Updates an existing release.
    /// </summary>
    /// <param name="id">The ID of the release to update.</param>
    /// <param name="release">The updated release data.</param>
    /// <returns>The updated release if found, null if the release doesn't exist or is deleted.</returns>
    public async Task<Release?> UpdateReleaseAsync(int id, Release release)
    {
        var existingRelease = await this.context.Releases.FindAsync(id);
        if (existingRelease == null || existingRelease.Deleted)
        {
            return null;
        }

        existingRelease.Title = release.Title;
        existingRelease.Description = release.Description;
        existingRelease.UserId = release.UserId;
        existingRelease.Type = release.Type;
        existingRelease.ReleaseDate = release.ReleaseDate;
        
        await this.context.SaveChangesAsync();
        return existingRelease;
    }

    /// <summary>
    /// Soft deletes a release.
    /// </summary>
    /// <param name="id">The ID of the release to delete.</param>
    /// <returns>True if the release was found and deleted, false otherwise.</returns>
    public async Task<bool> DeleteReleaseAsync(int id)
    {
        var release = await this.context.Releases.FindAsync(id);
        if (release == null || release.Deleted)
        {
            return false;
        }

        release.Deleted = true;
        await this.context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// Checks if a release exists and is not deleted.
    /// </summary>
    /// <param name="id">The ID of the release to check.</param>
    /// <returns>True if the release exists and is not deleted, false otherwise.</returns>
    public bool ReleaseExists(int id)
    {
        return this.context.Releases.Any(e => e.ReleaseId == id && !e.Deleted);
    }
} 