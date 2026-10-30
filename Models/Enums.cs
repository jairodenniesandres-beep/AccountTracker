using System.ComponentModel.DataAnnotations;

namespace AccountTracker.Models;

public enum TwoFactorMethod
{
    None,
    [Display(Name = "Authenticator App")] AuthenticatorApp,
    [Display(Name = "SMS")] Sms,
    [Display(Name = "Security Key")] SecurityKey,
    [Display(Name = "Google Prompt")] GooglePrompt
}

public enum AccountPurpose
{
    Personal,
    Work,
    School,
    Business,
    Testing,
    Other
}

public enum AccountStatus
{
    Active,
    Inactive,
    Suspended,
    [Display(Name = "Recovery Needed")] RecoveryNeeded
}
