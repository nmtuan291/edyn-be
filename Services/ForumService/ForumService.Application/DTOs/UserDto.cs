namespace ForumService.ForumService.Application.DTOs;

public class UserDto
{
    Guid Id { get; set; }
    string Username { get; set; }
    string Avatar { get; set; }
    string Bio { get; set; }
    DateTime BirthDay { get; set; }
    int? Gender { get; set; }
}