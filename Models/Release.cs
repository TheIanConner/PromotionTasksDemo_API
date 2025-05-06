using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PromotionTasksService.Models;

public class Release
{
    [Key]
    public int ReleaseId { get; set; }
    
    [Required]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;
    
    [Required]
    public ReleaseType Type { get; set; }
    
    [Required]
    public DateTime ReleaseDate { get; set; }
    
    [StringLength(1000)]
    public string? Description { get; set; }
    
    public bool Deleted { get; set; }
    
    public ICollection<PromotionTask> PromotionTasks { get; set; } = new List<PromotionTask>();
}

public enum ReleaseType
{
    Single,
    EP,
    Album,
    Mixtape
} 