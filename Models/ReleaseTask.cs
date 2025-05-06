using System.ComponentModel.DataAnnotations;

namespace PromotionTasksService.Models;

public class ReleaseTask
{
    [Key]
    public int ReleaseTaskId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    [Required]
    public TaskPriority Priority { get; set; }
    
    public bool Deleted { get; set; }
} 