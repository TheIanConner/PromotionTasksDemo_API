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
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReleaseService> _logger;

    public ReleaseService(ApplicationDbContext context, ILogger<ReleaseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Release?> GetReleaseByIdAsync(int id)
    {
        return await _context.Releases
            .FirstOrDefaultAsync(r => r.ReleaseId == id && !r.Deleted);
    }

    public async Task<List<Release>> GetUserReleasesAsync(int userId)
    {
        return await _context.Releases
            .Where(r => r.UserId == userId && !r.Deleted)
            .ToListAsync();
    }

    public async Task<Release> CreateReleaseAsync(Release release)
    {
        _context.Releases.Add(release);
        await _context.SaveChangesAsync();
        return release;
    }

    public async Task<Release?> UpdateReleaseAsync(int id, Release release)
    {
        var existingRelease = await _context.Releases.FindAsync(id);
        if (existingRelease == null || existingRelease.Deleted)
        {
            return null;
        }

        existingRelease.Title = release.Title;
        existingRelease.Description = release.Description;
        existingRelease.UserId = release.UserId;
        existingRelease.Type = release.Type;
        existingRelease.ReleaseDate = release.ReleaseDate;
        
        await _context.SaveChangesAsync();
        return existingRelease;
    }

    public async Task<bool> DeleteReleaseAsync(int id)
    {
        var release = await _context.Releases.FindAsync(id);
        if (release == null || release.Deleted)
        {
            return false;
        }

        release.Deleted = true;
        await _context.SaveChangesAsync();
        return true;
    }

    public bool ReleaseExists(int id)
    {
        return _context.Releases.Any(e => e.ReleaseId == id && !e.Deleted);
    }
} 