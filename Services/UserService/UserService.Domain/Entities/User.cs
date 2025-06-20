using System.ComponentModel.DataAnnotations;

namespace UserService.UserService.Domain.Entities;

public class User
{
    [Key]
    public string AccountId { get; set; }
    
    public DateTime? Birthday { get; set; }
    
    [Required]
    public string Avatar { get; set; }
    
    public string? Bio { get; set; } 
}