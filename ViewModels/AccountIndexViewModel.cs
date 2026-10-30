using System.ComponentModel.DataAnnotations;
using AccountTracker.Models;

namespace AccountTracker.ViewModels;

public static class AccountSort
{
    public const string EmailAsc = "email_asc";
    public const string EmailDesc = "email_desc";
    public const string PasswordOldest = "pwchange_asc";
    public const string PasswordNewest = "pwchange_desc";
    public const string CreatedNewest = "created_desc";
    public const string CreatedOldest = "created_asc";

    public static readonly (string Value, string Text)[] Options =
    {
        (EmailAsc, "Email (A-Z)"),
        (EmailDesc, "Email (Z-A)"),
        (PasswordOldest, "Password change (oldest first)"),
        (PasswordNewest, "Password change (newest first)"),
        (CreatedNewest, "Date created (newest first)"),
        (CreatedOldest, "Date created (oldest first)"),
    };
}

public class AccountFilterViewModel
{
    public AccountPurpose? Purpose { get; set; }
    public AccountStatus? Status { get; set; }

    /// <summary>"yes", "no" or empty.</summary>
    [Display(Name = "2FA")]
    [RegularExpression("^(yes|no)?$", ErrorMessage = "2FA filter must be 'yes' or 'no' - filter ignored.")]
    public string? TwoFactor { get; set; }

    [StringLength(100, ErrorMessage = "Search text cannot exceed 100 characters.")]
    public string? Search { get; set; }

    public string? Sort { get; set; }
}

public class AccountIndexViewModel
{
    public AccountFilterViewModel Filter { get; set; } = new();
    public IList<GmailAccount> Accounts { get; set; } = new List<GmailAccount>();
    public bool IsFiltered { get; set; }
    public DateTime Today { get; set; } = DateTime.Today;
}

public class AccountDetailsViewModel
{
    public GmailAccount Account { get; set; } = default!;
    public bool HasPassword { get; set; }
    /// <summary>Only set after the user POSTs "Reveal password".</summary>
    public string? RevealedPassword { get; set; }
    public string? RevealError { get; set; }
    public DateTime Today { get; set; } = DateTime.Today;
}
