namespace AuthService.AuthService.Application.Dtos;

public record LoginResponseDto
{
    public required string Id { get; init; }
    public required string UserName { get; init; }
    public required string Email { get; init; }
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
}