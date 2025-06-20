namespace UserService.UserService.Application.Dtos;

public class UserProfileDto
{
    public string? AccountId { get; set; }
    public required string UserName { get; set; }
    public DateTime Birthday { get; set; }
    public required string Avatar { get; set; }
    public string? Bio { get; set; } 
}