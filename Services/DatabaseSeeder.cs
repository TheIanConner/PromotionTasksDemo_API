using Microsoft.EntityFrameworkCore;
using PromotionTasksService.Data;
using PromotionTasksService.Models;

namespace PromotionTasksService.Services;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly Random _random = new Random();

    /// <summary>
    /// I've created this class to seed the database with some initial data the first time the app is run.
    /// This is useful for demo purposes to have some data to work with.
    /// I used an LLM to generate the initial data to save time.
    ///     
    /// Initializes a new instance of the DatabaseSeeder class.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="logger">The logger for logging messages.</param>
    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            await _context.Database.EnsureCreatedAsync();

            // Seed template tasks if they don't exist
            // These are the tasks that will be used to create promotion tasks for each release, so this is like a master list of tasks.
            if (!await _context.ReleaseTasks.AnyAsync())
            {
                var templateTasks = new List<ReleaseTask>
                {
                    new ReleaseTask
                    {
                        Title = "Social Media Announcement",
                        Description = "Create and post announcement across all social media platforms",
                        Priority = TaskPriority.High
                    },
                    new ReleaseTask
                    {
                        Title = "Playlist Pitching",
                        Description = "Research and pitch to relevant Spotify and Apple Music playlists",
                        Priority = TaskPriority.High
                    },
                    new ReleaseTask
                    {
                        Title = "Fan Email Campaign",
                        Description = "Send release announcement to fan mailing list",
                        Priority = TaskPriority.Medium
                    },
                    new ReleaseTask
                    {
                        Title = "Press Release",
                        Description = "Write and distribute press release to music blogs and media",
                        Priority = TaskPriority.Medium
                    },
                    new ReleaseTask
                    {
                        Title = "Music Video",
                        Description = "Plan and create music video for the release",
                        Priority = TaskPriority.High
                    },
                    new ReleaseTask
                    {
                        Title = "Radio Promotion",
                        Description = "Submit to radio stations and online radio shows",
                        Priority = TaskPriority.Medium
                    },
                    new ReleaseTask
                    {
                        Title = "Merchandise",
                        Description = "Design and order merchandise for the release",
                        Priority = TaskPriority.Low
                    },
                    new ReleaseTask
                    {
                        Title = "Live Performance",
                        Description = "Plan and schedule release party or live performance",
                        Priority = TaskPriority.Medium
                    },
                    new ReleaseTask
                    {
                        Title = "Collaboration Outreach",
                        Description = "Reach out to other artists for potential collaborations",
                        Priority = TaskPriority.Low
                    },
                    new ReleaseTask
                    {
                        Title = "Content Creation",
                        Description = "Create behind-the-scenes content and teasers",
                        Priority = TaskPriority.Medium
                    }
                };

                await _context.ReleaseTasks.AddRangeAsync(templateTasks);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Added {Count} template tasks to the database", templateTasks.Count);
            }

            // Check if users already exist
            var existingUsers = await _context.Users.ToListAsync();
            var userNames = existingUsers.Select(u => u.Name).ToList();

            // Just 2 test users for now
            var usersToAdd = new List<User>
            {
                new User { Name = "Ian Conner", CreatedDate = DateTime.UtcNow, LastActiveDate = DateTime.UtcNow },
                new User { Name = "Test McApp", CreatedDate = DateTime.UtcNow, LastActiveDate = DateTime.UtcNow }
            };

            // Only add users that don't already exist
            var newUsers = usersToAdd.Where(u => !userNames.Contains(u.Name)).ToList();

            if (newUsers.Any())
            {
                await _context.Users.AddRangeAsync(newUsers);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Added {Count} new users to the database", newUsers.Count);
            }
            else
            {
                _logger.LogInformation("No new users to add - all users already exist");
            }

            // Get all user IDs (including newly added ones)
            var allUserIds = await _context.Users
                .Select(u => u.UserId)
                .ToListAsync();

            // Check which users don't have releases
            var usersWithoutReleases = await _context.Users
                .Where(u => !_context.Releases.Any(r => r.UserId == u.UserId))
                .Select(u => u.UserId)
                .ToListAsync();

            foreach (var userId in usersWithoutReleases)
            {
                var numberOfReleases = _random.Next(1, 4); // Random number of releases for the users between 1 and 3
                var releases = new List<Release>();

                for (int i = 0; i < numberOfReleases; i++)
                {
                    var release = new Release
                    {
                        UserId = userId,
                        Title = GetRandomReleaseTitle(userId, i),
                        Type = GetRandomReleaseType(),
                        ReleaseDate = DateTime.UtcNow.AddDays(_random.Next(1, 365)), // Random date within next year
                        Description = GetRandomDescription()
                    };
                    releases.Add(release);
                }

                await _context.Releases.AddRangeAsync(releases);
                await _context.SaveChangesAsync(); // Save to get the ReleaseIds

                // Create promotion tasks for each release
                var templateTasks = await _context.ReleaseTasks.ToListAsync();
                var promotionTasks = new List<PromotionTask>();

                foreach (var release in releases)
                {
                    foreach (var templateTask in templateTasks)
                    {
                        var promotionTask = new PromotionTask
                        {
                            ReleaseId = release.ReleaseId,
                            Description = templateTask.Description,
                            Priority = templateTask.Priority,
                            Status = PromotionTaskStatus.ToDo,
                            DueDate = release.ReleaseDate.AddDays(_random.Next(1, 30)) // Random due date within 30 days of release
                        };
                        promotionTasks.Add(promotionTask);
                    }
                }

                await _context.PromotionTasks.AddRangeAsync(promotionTasks);
                _logger.LogInformation("Added {Count} releases and {TaskCount} promotion tasks for user ID {UserId}", 
                    releases.Count, promotionTasks.Count, userId);
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private string GetRandomReleaseTitle(int userId, int index)
    {
        var titles = new[]
        {
            "Summer Vibes",
            "Midnight Sessions",
            "Urban Dreams",
            "Lost in Time",
            "Future Sounds",
            "Acoustic Journey",
            "Electric Soul",
            "Neon Nights",
            "Digital Dawn"
        };

        return index < titles.Length ? titles[index] : $"Release {_random.Next(0, titles.Length)}";
    }

    private ReleaseType GetRandomReleaseType()
    {
        var types = Enum.GetValues<ReleaseType>();
        return types[_random.Next(types.Length)];
    }

    private string GetRandomDescription()
    {
        var descriptions = new[]
        {
            "A groundbreaking new release that pushes musical boundaries.",
            "An intimate collection of songs exploring themes of love and loss.",
            "High-energy tracks perfect for any playlist.",
            "A carefully crafted album showcasing artistic growth.",
            "An experimental journey through sound and emotion."
        };

        return descriptions[_random.Next(descriptions.Length)];
    }
} 