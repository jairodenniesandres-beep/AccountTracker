using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace AccountTracker.Models;

/// <summary>
/// Database entity. Validation lives on <see cref="ViewModels.AccountFormViewModel"/>;
/// the password is only ever stored encrypted (see <see cref="Services.IPasswordProtector"/>).
/// </summary>
public class GmailAccount
{
    public int Id { get; set; }

    /// <summary>Owning Identity user. Accounts are private to this user — never shared with Admin or others.</summary>
    [Required]
    public string OwnerUserId { get; set; } = string.Empty;

    [ForeignKey(nameof(OwnerUserId))]
    public IdentityUser? Owner { get; set; }

    /// <summary>Stored trimmed and lower-case, so the unique index is case-insensitive per owner.</summary>
    [Display(Name = "Email Address")]
    public string EmailAddress { get; set; } = string.Empty;

    [Display(Name = "Display Name")]
    public string? DisplayName { get; set; }

    /// <summary>Data Protection payload (base64url). Never plaintext.</summary>
    public string? EncryptedPassword { get; set; }

    [Display(Name = "Recovery Email")]
    public string? RecoveryEmail { get; set; }

    [Display(Name = "Recovery Phone")]
    public string? RecoveryPhone { get; set; }

    [Display(Name = "2FA Enabled")]
    public bool TwoFactorEnabled { get; set; }

    [Display(Name = "2FA Method")]
    public TwoFactorMethod TwoFactorMethod { get; set; }

    public AccountPurpose Purpose { get; set; }

    public AccountStatus Status { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Date Created")]
    public DateTime? DateCreated { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Last Password Change")]
    public DateTime? LastPasswordChange { get; set; }

    [DataType(DataType.DateTime)]
    [Display(Name = "Last Login")]
    public DateTime? LastLogin { get; set; }

    public string? Notes { get; set; }

    [Display(Name = "Record Created")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Display(Name = "Record Updated")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Optimistic concurrency token, regenerated on every update.</summary>
    [ConcurrencyCheck]
    public Guid Version { get; set; } = Guid.NewGuid();

    public bool NeedsPasswordUpdate(DateTime today) =>
        LastPasswordChange == null || LastPasswordChange.Value.Date < today.AddYears(-1);
}
