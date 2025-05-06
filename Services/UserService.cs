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
    /// <summary>
    /// Gets the database context.
    /// </summary>
    private readonly ApplicationDbContext context;

    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    private readonly ILogger<UserService> logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserService"/> class.
    /// </summary>
    /// <param name="context">The database context.</param>
    /// <param name="logger">The logger instance.</param>
    public UserService(ApplicationDbContext context, ILogger<UserService> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    /// <summary>
    /// Gets all active users from the database.
    /// </summary>
    /// <returns>A list of all active users.</returns>
    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        var users = await this.context.Users
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
            await this.context.SaveChangesAsync();
        }
        
        return users;
    }

    /// <summary>
    /// Gets a user by their ID, including their releases and tasks.
    /// </summary>
    /// <param name="id">The ID of the user to retrieve.</param>
    /// <returns>The user if found, null otherwise.</returns>
    public async Task<User?> GetUserByIdWithReleasesAndTasksAsync(int id)
    {
        var user = await this.context.Users
            .Include(u => u.Releases)
                .ThenInclude(r => r.PromotionTasks)
            .Where(u => u.UserId == id && !u.Deleted)
            .FirstOrDefaultAsync();
        
        if (user != null)
        {
            user.LastActiveDate = DateTime.UtcNow;
            await this.context.SaveChangesAsync();
        }
        
        return user;
    }

    /// <summary>
    /// Gets a user by their name, including their releases and tasks.
    /// </summary>
    /// <param name="name">The name of the user to retrieve.</param>
    /// <returns>The user if found, null otherwise.</returns>
    public async Task<User?> GetUserByNameWithReleasesAndTasksAsync(string name)
    {
        var user = await this.context.Users
            .Include(u => u.Releases)
                .ThenInclude(r => r.PromotionTasks)
            .Where(u => u.Name.Equals(name) && !u.Deleted)
            .FirstOrDefaultAsync();
        
        if (user != null)
        {
            user.LastActiveDate = DateTime.UtcNow;
            await this.context.SaveChangesAsync();
        }
        
        return user;
    }
} 