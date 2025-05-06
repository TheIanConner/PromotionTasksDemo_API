using Microsoft.EntityFrameworkCore;
using PromotionTasksService.Data;
using PromotionTasksService.Models;

namespace PromotionTasksService.Services;

/// <summary>
/// This service is for managing Users. Users have a name and a list of releases.
/// It uses SQLite for the database and Entity Framework Core for the ORM.
/// TODO: Allow a soft delete of a user.
/// </summary>
public class UserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Where(u => !u.Deleted)
            .OrderBy(u => u.Name)
            .ToListAsync();

        // Update LastActiveDate for all users
        foreach (var user in users)
        {
            user.LastActiveDate = DateTime.UtcNow;
        }
        
        if (users.Any())
        {
            await _context.SaveChangesAsync();
        }
        
        return users;
    }

    public async Task<User?> GetUserByIdWithReleasesAndTasksAsync(int id)
    {
        var user = await _context.Users
            .Include(u => u.Releases)
                .ThenInclude(r => r.PromotionTasks)
            .Where(u => u.UserId == id && !u.Deleted)
            .FirstOrDefaultAsync();
        
        if (user != null)
        {
            user.LastActiveDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        
        return user;
    }

    public async Task<User?> GetUserByNameWithReleasesAndTasksAsync(string name)
    {
        var user = await _context.Users
            .Include(u => u.Releases)
                .ThenInclude(r => r.PromotionTasks)
            .Where(u => u.Name.Equals(name) && !u.Deleted)
            .FirstOrDefaultAsync();
        
        if (user != null)
        {
            user.LastActiveDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        
        return user;
    }
} 