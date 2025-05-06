using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PromotionTasksService.Models;

/// <summary>
/// Represents a promotion task for a music release.
/// </summary>
public class PromotionTask
{
    /// <summary>
    /// Gets or sets the unique identifier for the task.
    /// </summary>
    [Key]
    public int TaskId { get; set; }
    
    /// <summary>
    /// Gets or sets the ID of the release this task belongs to.
    /// </summary>
    [Required]
    public int ReleaseId { get; set; }
    
    /// <summary>
    /// Gets or sets the release this task belongs to.
    /// </summary>
    [JsonIgnore]
    public Release? Release { get; set; }
    
    /// <summary>
    /// Gets or sets the status of the task.
    /// </summary>
    [Required]
    public PromotionTaskStatus Status { get; set; }
    
    /// <summary>
    /// Gets or sets the priority level of the task.
    /// </summary>
    [Required]
    public TaskPriority Priority { get; set; }
    
    /// <summary>
    /// Gets or sets the description of the task.
    /// </summary>
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the due date for the task.
    /// </summary>
    public DateTime? DueDate { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the task has been deleted.
    /// </summary>
    public bool Deleted { get; set; }
} 