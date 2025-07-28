using System.IdentityModel.Tokens.Jwt;
using AuthService.AuthService.Application;
using Moq;
using Xunit;


namespace AuthService.Tests.UnitTests;

public class TokenServiceTest
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly TokenService _tokenService;

    public TokenServiceTest()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(x => x["Jwt:Issuer"]).Returns("issuer.jwt");
        _mockConfiguration.Setup(x => x["Jwt:Audience"]).Returns("audience.jwt");
        _mockConfiguration.Setup(x => x["Jwt:Key"]).Returns("DD9oeRNZcvsFpzYAaz0kf5bzx1N6MxNY");
        _tokenService = new TokenService(_mockConfiguration.Object);
    }
    
    [Fact]
    public void GenerateJwtToken_ReturnsTokenString()
    {
        // Arrange
        string userId = Guid.NewGuid().ToString();
        string email = "testemail@mail.com";
        string username = "testuser";
        // Act
        string token = _tokenService.GenerateJwtToken(userId, email, username);
        
        //Assert
        Assert.NotNull(token);
        Assert.NotEmpty(token);

        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
        
        Assert.NotNull(jsonToken);
        Assert.Equal(userId, jsonToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
        Assert.Equal(email, jsonToken.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
    }
}