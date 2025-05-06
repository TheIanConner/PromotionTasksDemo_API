using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PromotionTasksService.Models;

public class PromotionTask
{
    [Key]
    public int TaskId { get; set; }
    
    [Required]
    public int ReleaseId { get; set; }
    
    [ForeignKey("ReleaseId")]
    public Release Release { get; set; } = null!;
    
    [Required]
    public PromotionTaskStatus Status { get; set; }
    
    [Required]
    public TaskPriority Priority { get; set; }
    
    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;
    
    public DateTime? DueDate { get; set; }
    
    public bool Deleted { get; set; }
}

public enum TaskPriority
{
    Low,
    Medium,
    High,
    Urgent
} 