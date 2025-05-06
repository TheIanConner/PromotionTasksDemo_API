using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PromotionTasksService.Models;

/// <summary>
/// Represents a music release in the system.
/// </summary>
public class Release
{
    /// <summary>
    /// Gets or sets the unique identifier for the release.
    /// </summary>
    [Key]
    public int ReleaseId { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the user who owns this release.
    /// </summary>
    [Required]
    public int UserId { get; set; }
    
    /// <summary>
    /// Gets or sets the title of the release.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the type of the release (Single, EP, Album, or Mixtape).
    /// </summary>
    [Required]
    public ReleaseType Type { get; set; }
    
    /// <summary>
    /// Gets or sets the planned release date.
    /// </summary>
    [Required]
    public DateTime ReleaseDate { get; set; }
    
    /// <summary>
    /// Gets or sets the description of the release.
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the release has been deleted.
    /// </summary>
    public bool Deleted { get; set; }
    
    /// <summary>
    /// Gets or sets the collection of promotion tasks associated with this release.
    /// </summary>
    public ICollection<PromotionTask> PromotionTasks { get; set; } = new List<PromotionTask>();
} 