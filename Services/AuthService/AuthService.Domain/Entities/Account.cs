using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AuthService.AuthService.Domain.Entities;

public class Account: IdentityUser
{
    public string? RefreshToken { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}