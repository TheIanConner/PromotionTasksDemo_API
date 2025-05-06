using Microsoft.EntityFrameworkCore;
using PromotionTasksService.Models;

namespace PromotionTasksService.Data;

/// <summary>
/// The main database context for the application.
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to be used by the context.</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    /// <summary>
    /// Gets or sets the users in the database.
    /// </summary>
    public DbSet<User> Users { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the releases in the database.
    /// </summary>
    public DbSet<Release> Releases { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the promotion tasks in the database.
    /// </summary>
    public DbSet<PromotionTask> PromotionTasks { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets the release task templates in the database.
    /// </summary>
    public DbSet<ReleaseTask> ReleaseTasks { get; set; } = null!;
    
    /// <summary>
    /// Configures the model that was discovered by convention from the entity types.
    /// </summary>
    /// <param name="modelBuilder">The builder being used to construct the model for this context.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Name)
            .IsUnique();
            
        modelBuilder.Entity<PromotionTask>()
            .HasOne(p => p.Release)
            .WithMany(r => r.PromotionTasks)
            .HasForeignKey(p => p.ReleaseId)
            .IsRequired();
    }
} 