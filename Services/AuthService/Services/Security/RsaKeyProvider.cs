using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services.Security;

public class RsaKeyProvider : IDisposable
{
    private readonly ConcurrentDictionary<string, RsaSecurityKey> _keyCache = new();
    private readonly IHostEnvironment _env;
    private bool _disposed;

    public RsaKeyProvider(IHostEnvironment env)
    {
        _env = env;
    }

    private static string NormalizePemFromEnv(string pem)
    {
        return pem
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace("\r", "\n", StringComparison.Ordinal)
            .Replace("\\n", "\n", StringComparison.Ordinal);
    }

    /// <summary>
    /// Absolute paths are used as-is; relative paths are resolved under <see cref="IHostEnvironment.ContentRootPath"/>.
    /// </summary>
    private string ResolveKeyPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("RSA key path is not configured.", nameof(path));

        if (Path.IsPathRooted(path))
            return path;

        var combined = Path.Combine(_env.ContentRootPath, path.TrimStart('.', '/', '\\'));
        return Path.GetFullPath(combined);
    }

    public RsaSecurityKey GetPrivateKey(string privatePemPath, string keyId)
    {
        var resolved = ResolveKeyPath(privatePemPath);
        return _keyCache.GetOrAdd($"private:{resolved}:{keyId}", _ =>
        {
            var pem = File.ReadAllText(resolved);
            return CreatePrivateKeyFromPem(pem, keyId);
        });
    }

    public RsaSecurityKey GetPrivateKey(IConfigurationSection rsa, string keyId)
    {
        var inline = rsa["PrivateKeyPem"];
        if (!string.IsNullOrWhiteSpace(inline))
        {
            var normalized = NormalizePemFromEnv(inline);
            return _keyCache.GetOrAdd($"private:pem:{keyId}", _ => CreatePrivateKeyFromPem(normalized, keyId));
        }

        return GetPrivateKey(rsa["PrivateKeyDirectory"] ?? string.Empty, keyId);
    }

    public RsaSecurityKey GetPublicKey(string publicPemPath, string keyId)
    {
        var resolved = ResolveKeyPath(publicPemPath);
        return _keyCache.GetOrAdd($"public:{resolved}:{keyId}", _ =>
        {
            var pem = File.ReadAllText(resolved);
            return CreatePublicKeyFromPem(pem, keyId);
        });
    }

    public RsaSecurityKey GetPublicKey(IConfigurationSection rsa, string keyId)
    {
        var inline = rsa["PublicKeyPem"];
        if (!string.IsNullOrWhiteSpace(inline))
        {
            var normalized = NormalizePemFromEnv(inline);
            return _keyCache.GetOrAdd($"public:pem:{keyId}", _ => CreatePublicKeyFromPem(normalized, keyId));
        }

        return GetPublicKey(rsa["PublicKeyDirectory"] ?? string.Empty, keyId);
    }

    private static RsaSecurityKey CreatePrivateKeyFromPem(string pem, string keyId)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem.AsSpan());
        return new RsaSecurityKey(rsa) { KeyId = keyId };
    }

    private static RsaSecurityKey CreatePublicKeyFromPem(string pem, string keyId)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem.AsSpan());
        return new RsaSecurityKey(rsa) { KeyId = keyId };
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        foreach (var key in _keyCache.Values)
        {
            key.Rsa?.Dispose();
        }
        _keyCache.Clear();
    }
}
