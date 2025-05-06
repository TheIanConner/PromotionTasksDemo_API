using System.ComponentModel.DataAnnotations;

namespace PromotionTasksService.Models;

public class User
{
    [Key]
    public int UserId { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime LastActiveDate { get; set; } = DateTime.UtcNow;
    
    public bool Deleted { get; set; }
    
    public ICollection<Release> Releases { get; set; } = new List<Release>();
} 