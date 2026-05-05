namespace AuthService.AuthService.Application.Dtos;

public record AccountDto
{
    public required string Id { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required bool IsActive { get; init; }
}