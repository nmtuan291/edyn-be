using System.Security.Cryptography;
using AuthService.Data;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services.Sercurity;

public class RsaKeyProvider
{
    public RsaSecurityKey GetPrivateKey(string privatePemPath, string keyId)
    {
        var pem = File.ReadAllText(privatePemPath);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem.ToCharArray());
        RsaSecurityKey privateKey = new(rsa) { KeyId = keyId };
        return privateKey;
    }

    public RsaSecurityKey GetPublicKey(string publicPemPath, string keyId)
    {
        var pem = File.ReadAllText(publicPemPath);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem.ToCharArray());
        RsaSecurityKey publicKey = new(rsa) { KeyId = keyId };
        return publicKey;
    }
}