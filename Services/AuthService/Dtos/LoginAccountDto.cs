using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public class LoginAccountDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required bool IsEmail { get;set; }
    
}