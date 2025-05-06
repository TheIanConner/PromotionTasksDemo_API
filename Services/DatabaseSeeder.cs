using Microsoft.EntityFrameworkCore;
using PromotionTasksService.Data;
using PromotionTasksService.Models;

namespace PromotionTasksService.Services;

/// <summary>
/// Seeds the database with initial data for demo purposes.
/// </summary>
public class DatabaseSeeder
{
    /// <summary>
    /// Gets the database context.
    /// </summary>
    private readonly ApplicationDbContext context;

    /// <summary>
    /// Gets the logger instance.
    /// </summary>
    private readonly ILogger<DatabaseSeeder> logger;

    /// <summary>
    /// Gets the random number generator.
    /// </summary>
    private readonly Random random = new Random();

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseSeeder"/> class.
    /// This class seeds the database with initial data the first time the app is run.
    /// This is useful for demo purposes to have some data to work with.
    /// The initial data was generated using an LLM to save time.
    /// </summary>
    /// <param name="context">The application database context.</param>
    /// <param name="logger">The logger for logging messages.</param>
    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    /// <summary>
    /// Seeds the database with initial data if it doesn't exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SeedAsync()
    {
        try
        {
            await this.context.Database.EnsureCreatedAsync();

            // Seed template tasks if they don't exist
            // These are the tasks that will be used to create promotion tasks for each release, so this is like a master list of tasks.
            if (!await this.context.ReleaseTasks.AnyAsync())
            {
                var templateTasks = new List<ReleaseTask>
                {
                    new ReleaseTask
                    {
                        Title = "Social Media Announcement",
                        Description = "Create and post announcement across all social media platforms",
                        Priority = TaskPriority.High,
                    },
                    new ReleaseTask
                    {
                        Title = "Playlist Pitching",
                        Description = "Research and pitch to relevant Spotify and Apple Music playlists",
                        Priority = TaskPriority.High,
                    },
                    new ReleaseTask
                    {
                        Title = "Fan Email Campaign",
                        Description = "Send release announcement to fan mailing list",
                        Priority = TaskPriority.Medium,
                    },
                    new ReleaseTask
                    {
                        Title = "Press Release",
                        Description = "Write and distribute press release to music blogs and media",
                        Priority = TaskPriority.Medium,
                    },
                    new ReleaseTask
                    {
                        Title = "Music Video",
                        Description = "Plan and create music video for the release",
                        Priority = TaskPriority.High,
                    },
                    new ReleaseTask
                    {
                        Title = "Radio Promotion",
                        Description = "Submit to radio stations and online radio shows",
                        Priority = TaskPriority.Medium,
                    },
                    new ReleaseTask
                    {
                        Title = "Merchandise",
                        Description = "Design and order merchandise for the release",
                        Priority = TaskPriority.Low,
                    },
                    new ReleaseTask
                    {
                        Title = "Live Performance",
                        Description = "Plan and schedule release party or live performance",
                        Priority = TaskPriority.Medium,
                    },
                    new ReleaseTask
                    {
                        Title = "Collaboration Outreach",
                        Description = "Reach out to other artists for potential collaborations",
                        Priority = TaskPriority.Low,
                    },
                    new ReleaseTask
                    {
                        Title = "Content Creation",
                        Description = "Create behind-the-scenes content and teasers",
                        Priority = TaskPriority.Medium,
                    },
                };

                await this.context.ReleaseTasks.AddRangeAsync(templateTasks);
                await this.context.SaveChangesAsync();
                this.logger.LogInformation("Added {Count} template tasks to the database", templateTasks.Count);
            }

            // Check if users already exist
            var existingUsers = await this.context.Users.ToListAsync();
            var userNames = existingUsers.Select(u => u.Name).ToList();

            // Just 2 test users for now
            var usersToAdd = new List<User>
            {
                new User { Name = "Ian Conner", CreatedDate = DateTime.UtcNow, LastActiveDate = DateTime.UtcNow },
                new User { Name = "Test McApp", CreatedDate = DateTime.UtcNow, LastActiveDate = DateTime.UtcNow },
            };

            // Only add users that don't already exist
            var newUsers = usersToAdd.Where(u => !userNames.Contains(u.Name)).ToList();

            if (newUsers.Any())
            {
                await this.context.Users.AddRangeAsync(newUsers);
                await this.context.SaveChangesAsync();
                this.logger.LogInformation("Added {Count} new users to the database", newUsers.Count);
            }
            else
            {
                this.logger.LogInformation("No new users to add - all users already exist");
            }

            // Get all user IDs (including newly added ones)
            var allUserIds = await this.context.Users
                .Select(u => u.UserId)
                .ToListAsync();

            // Check which users don't have releases
            var usersWithoutReleases = await this.context.Users
                .Where(u => !this.context.Releases.Any(r => r.UserId == u.UserId))
                .Select(u => u.UserId)
                .ToListAsync();

            foreach (var userId in usersWithoutReleases)
            {
                var numberOfReleases = this.random.Next(1, 4); // Random number of releases for the users between 1 and 3
                var releases = new List<Release>();

                for (int i = 0; i < numberOfReleases; i++)
                {
                    var release = new Release
                    {
                        UserId = userId,
                        Title = this.GetRandomReleaseTitle(userId, i),
                        Type = this.GetRandomReleaseType(),
                        ReleaseDate = DateTime.UtcNow.AddDays(this.random.Next(1, 365)), // Random date within next year
                        Description = this.GetRandomDescription(),
                    };
                    releases.Add(release);
                }

                await this.context.Releases.AddRangeAsync(releases);
                await this.context.SaveChangesAsync(); // Save to get the ReleaseIds

                // Create promotion tasks for each release
                var templateTasks = await this.context.ReleaseTasks.ToListAsync();
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
                            DueDate = release.ReleaseDate.AddDays(this.random.Next(1, 30)), // Random due date within 30 days of release
                        };
                        promotionTasks.Add(promotionTask);
                    }
                }

                await this.context.PromotionTasks.AddRangeAsync(promotionTasks);
                await this.context.SaveChangesAsync();
                this.logger.LogInformation("Added {Count} promotion tasks to the database", promotionTasks.Count);
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error occurred while seeding the database");
            throw;
        }
    }

    /// <summary>
    /// Gets a random release title based on the user ID and index.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="index">The index of the release.</param>
    /// <returns>A random release title.</returns>
    private string GetRandomReleaseTitle(int userId, int index)
    {
        var titles = new[]
        {
            "Midnight Dreams",
            "Electric Soul",
            "Neon Nights",
            "Cosmic Journey",
            "Urban Echoes",
            "Digital Waves",
            "Silent Storm",
            "Golden Hour",
            "Crystal Clear",
            "Eternal Light",
        };

        return titles[(userId + index) % titles.Length];
    }

    /// <summary>
    /// Gets a random release type.
    /// </summary>
    /// <returns>A random release type.</returns>
    private ReleaseType GetRandomReleaseType()
    {
        var types = Enum.GetValues(typeof(ReleaseType));
        return (ReleaseType)types.GetValue(this.random.Next(types.Length))!;
    }

    /// <summary>
    /// Gets a random description for a release.
    /// </summary>
    /// <returns>A random description.</returns>
    private string GetRandomDescription()
    {
        var descriptions = new[]
        {
            "A journey through sound and emotion.",
            "Pushing the boundaries of contemporary music.",
            "An exploration of new sonic landscapes.",
            "Where rhythm meets melody in perfect harmony.",
            "A testament to musical innovation.",
        };

        return descriptions[this.random.Next(descriptions.Length)];
    }
} 