using System.ComponentModel.DataAnnotations;
using AccountTracker.Models;
using AccountTracker.Validation;

namespace AccountTracker.ViewModels;

/// <summary>Create/Edit form. The password is write-only: it is never sent back to the browser.</summary>
public class AccountFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    public Guid Version { get; set; }

    /// <summary>Set by the controller (not bound) - true on Edit, where a blank password keeps the current one.
    /// "Password required on Create" is checked in the controller.</summary>
    public bool IsEdit { get; set; }

    /// <summary>Set by the controller (not bound) - whether a password is currently stored.</summary>
    public bool HasStoredPassword { get; set; }

    [Required(ErrorMessage = "Email address is required.")]
    [StringLength(254, ErrorMessage = "Email address is too long.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [GmailAddress]
    [Display(Name = "Email Address")]
    public string EmailAddress { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 2, ErrorMessage = "Display name must be between 2 and 100 characters.")]
    [Display(Name = "Display Name")]
    public string? DisplayName { get; set; }

    [StringLength(200, ErrorMessage = "Password cannot exceed 200 characters.")]
    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [StringLength(254, ErrorMessage = "Recovery email is too long.")]
    [EmailAddress(ErrorMessage = "Enter a valid recovery email address.")]
    [Display(Name = "Recovery Email")]
    public string? RecoveryEmail { get; set; }

    [Phone(ErrorMessage = "Enter a valid phone number.")]
    [RegularExpression(@"^\+?[0-9][0-9 ()\-]{5,24}$", ErrorMessage = "Enter a valid phone number, e.g. +1 555 010 0001.")]
    [Display(Name = "Recovery Phone")]
    public string? RecoveryPhone { get; set; }

    [Display(Name = "2-Step Verification (2FA) enabled")]
    public bool TwoFactorEnabled { get; set; }

    [Required(ErrorMessage = "Please select a 2FA method.")]
    [EnumDataType(typeof(TwoFactorMethod), ErrorMessage = "Please select a valid 2FA method.")]
    [Display(Name = "2FA Method")]
    public TwoFactorMethod? TwoFactorMethod { get; set; } = Models.TwoFactorMethod.None;

    [Required(ErrorMessage = "Please select a purpose.")]
    [EnumDataType(typeof(AccountPurpose), ErrorMessage = "Please select a valid purpose.")]
    public AccountPurpose? Purpose { get; set; }

    [Required(ErrorMessage = "Please select a status.")]
    [EnumDataType(typeof(AccountStatus), ErrorMessage = "Please select a valid status.")]
    public AccountStatus? Status { get; set; } = AccountStatus.Active;

    [DataType(DataType.Date)]
    [NotInFuture]
    [Display(Name = "Date Created")]
    public DateTime? DateCreated { get; set; }

    [DataType(DataType.Date)]
    [NotInFuture]
    [Display(Name = "Last Password Change")]
    public DateTime? LastPasswordChange { get; set; }

    [DataType(DataType.DateTime)]
    [NotInFuture]
    [Display(Name = "Last Login")]
    public DateTime? LastLogin { get; set; }

    [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters.")]
    [DataType(DataType.MultilineText)]
    public string? Notes { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var method = TwoFactorMethod ?? Models.TwoFactorMethod.None;
        if (TwoFactorEnabled && method == Models.TwoFactorMethod.None)
            yield return new ValidationResult("Choose the 2FA method used when 2FA is enabled.", new[] { nameof(TwoFactorMethod) });
        if (!TwoFactorEnabled && method != Models.TwoFactorMethod.None)
            yield return new ValidationResult("2FA method must be 'None' when 2FA is disabled.", new[] { nameof(TwoFactorMethod) });

        if (!string.IsNullOrWhiteSpace(RecoveryEmail) &&
            string.Equals(RecoveryEmail.Trim(), EmailAddress?.Trim(), StringComparison.OrdinalIgnoreCase))
            yield return new ValidationResult("Recovery email must be different from the account's email address.", new[] { nameof(RecoveryEmail) });

        if (DateCreated.HasValue && LastPasswordChange.HasValue && LastPasswordChange.Value.Date < DateCreated.Value.Date)
            yield return new ValidationResult("Last password change cannot be before the date the account was created.", new[] { nameof(LastPasswordChange) });

        if (DateCreated.HasValue && LastLogin.HasValue && LastLogin.Value.Date < DateCreated.Value.Date)
            yield return new ValidationResult("Last login cannot be before the date the account was created.", new[] { nameof(LastLogin) });

    }
}
