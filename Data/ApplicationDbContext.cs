using Microsoft.EntityFrameworkCore;
using PromotionTasksService.Models;

namespace PromotionTasksService.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Release> Releases { get; set; } = null!;
    public DbSet<PromotionTask> PromotionTasks { get; set; } = null!;
    public DbSet<ReleaseTask> ReleaseTasks { get; set; } = null!;
    
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