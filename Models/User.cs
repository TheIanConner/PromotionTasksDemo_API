using System.ComponentModel.DataAnnotations;

namespace PromotionTasksService.Models;

/// <summary>
/// Represents a user in the system.
/// </summary>
public class User
{
    /// <summary>
    /// Gets or sets the unique identifier for the user.
    /// </summary>
    [Key]
    public int UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the user.
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the date when the user was created.
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the date when the user was last active.
    /// </summary>
    public DateTime LastActiveDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets a value indicating whether the user has been deleted.
    /// </summary>
    public bool Deleted { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of releases associated with this user.
    /// </summary>
    public ICollection<Release> Releases { get; set; } = new List<Release>();
} 