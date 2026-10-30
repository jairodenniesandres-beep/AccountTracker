using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;

namespace AccountTracker.Services;

public interface IPasswordProtector
{
    string Protect(string plaintext);

    /// <summary>Returns false if the payload can't be decrypted (e.g. the key ring was lost or replaced).</summary>
    bool TryUnprotect(string? protectedValue, out string? plaintext);
}

/// <summary>Encrypts account passwords with ASP.NET Core Data Protection.</summary>
public class PasswordProtector : IPasswordProtector
{
    // Changing this string makes all existing passwords unreadable - keep it stable.
    public const string Purpose = "AccountTracker.GmailAccount.Password.v1";

    private readonly IDataProtector _protector;
    private readonly ILogger<PasswordProtector> _logger;

    public PasswordProtector(IDataProtectionProvider provider, ILogger<PasswordProtector> logger)
    {
        _protector = provider.CreateProtector(Purpose);
        _logger = logger;
    }

    public string Protect(string plaintext) => _protector.Protect(plaintext);

    public bool TryUnprotect(string? protectedValue, out string? plaintext)
    {
        plaintext = null;
        if (string.IsNullOrEmpty(protectedValue)) return false;
        try
        {
            plaintext = _protector.Unprotect(protectedValue);
            return true;
        }
        catch (CryptographicException ex)
        {
            // Never log the payload or plaintext.
            _logger.LogWarning(ex, "A stored password could not be decrypted (Data Protection keys missing or changed).");
            return false;
        }
    }
}
