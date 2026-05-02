namespace ForumService.ForumService.Application.DTOs;

public class UserDto
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Avatar { get; set; }
    public string Bio { get; set; }
    public DateTime BirthDay { get; set; }
    public int? Gender { get; set; }
}