using System.ComponentModel.DataAnnotations;

namespace PromotionTasksService.Models;

/// <summary>
/// Represents a template for a promotion task that can be assigned to releases.
/// </summary>
public class ReleaseTask
{
    /// <summary>
    /// Gets or sets the unique identifier for the release task template.
    /// </summary>
    [Key]
    public int ReleaseTaskId { get; set; }
    
    /// <summary>
    /// Gets or sets the title of the task template.
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the description of what needs to be done for this task.
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the priority level of the task.
    /// </summary>
    [Required]
    public TaskPriority Priority { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the task template has been deleted.
    /// </summary>
    public bool Deleted { get; set; }
} 