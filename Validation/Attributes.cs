using System.ComponentModel.DataAnnotations;

namespace AccountTracker.Validation;

/// <summary>Fails when a date is later than today (server local time).</summary>
[AttributeUsage(AttributeTargets.Property)]
public class NotInFutureAttribute : ValidationAttribute
{
    public NotInFutureAttribute() : base("{0} cannot be in the future.") { }

    public override bool IsValid(object? value) =>
        value is not DateTime date || date.Date <= DateTime.Today;
}

/// <summary>Requires the address to be on gmail.com or googlemail.com.</summary>
[AttributeUsage(AttributeTargets.Property)]
public class GmailAddressAttribute : ValidationAttribute
{
    public static readonly string[] Domains = { "@gmail.com", "@googlemail.com" };

    public GmailAddressAttribute() : base("{0} must end with @gmail.com or @googlemail.com.") { }

    public override bool IsValid(object? value) =>
        value is not string s || string.IsNullOrWhiteSpace(s)
        || Domains.Any(d => s.Trim().EndsWith(d, StringComparison.OrdinalIgnoreCase) && s.Trim().Length > d.Length);
}
