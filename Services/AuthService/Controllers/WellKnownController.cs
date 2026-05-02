using AuthService.Interfaces.Services;
using AuthService.Services.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers;

[ApiController]
[Route(".well-known")]
public class WellKnownController : ControllerBase
{
    private readonly RsaSecurityKey _publicKey;
    private readonly IConfiguration _config;

    public WellKnownController(RsaKeyProvider rsaKeyProvider,  IConfiguration config)
    {
        _publicKey = rsaKeyProvider.GetPublicKey(config.GetSection("Rsa"), config["Rsa:Kid"]!);
        _config = config;
    }

    [HttpGet("jwks.json")]
    public IActionResult GetJwks()
    {
        var rsa = _publicKey.Rsa.ExportParameters(false);

        var jwks = new
        {
            kty = "RSA",
            use = "sig",
            kid = _publicKey.KeyId,
            alg = "RS256",
            n = Base64UrlEncoder.Encode(rsa.Modulus),
            e = Base64UrlEncoder.Encode(rsa.Exponent)
        };
        
        return Ok(new { keys = new [] { jwks } });
    }
    
    [HttpGet("openid-configuration")]
    public IActionResult Oidc()
    {
        var issuer = _config["Jwt:Issuer"];
        return Ok(new {
            issuer,
            jwks_uri = $"{issuer}/.well-known/jwks.json"
        });
    }
}