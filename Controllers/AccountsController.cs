using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using AccountTracker.Data;
using AccountTracker.Models;
using AccountTracker.Services;
using AccountTracker.ViewModels;

namespace AccountTracker.Controllers;

[Authorize]
public class AccountsController : Controller
{
    // Fields a user may post on Create/Edit (Id/Version only on Edit). IsEdit/HasStoredPassword are never bound.
    private const string CreateFields =
        nameof(AccountFormViewModel.EmailAddress) + "," + nameof(AccountFormViewModel.DisplayName) + "," +
        nameof(AccountFormViewModel.Password) + "," + nameof(AccountFormViewModel.RecoveryEmail) + "," +
        nameof(AccountFormViewModel.RecoveryPhone) + "," + nameof(AccountFormViewModel.TwoFactorEnabled) + "," +
        nameof(AccountFormViewModel.TwoFactorMethod) + "," + nameof(AccountFormViewModel.Purpose) + "," +
        nameof(AccountFormViewModel.Status) + "," + nameof(AccountFormViewModel.DateCreated) + "," +
        nameof(AccountFormViewModel.LastPasswordChange) + "," + nameof(AccountFormViewModel.LastLogin) + "," +
        nameof(AccountFormViewModel.Notes);
    private const string EditFields = nameof(AccountFormViewModel.Id) + "," + nameof(AccountFormViewModel.Version) + "," + CreateFields;

    private readonly AppDbContext _context;
    private readonly IPasswordProtector _passwords;
    private readonly ILogger<AccountsController> _logger;

    public AccountsController(AppDbContext context, IPasswordProtector passwords, ILogger<AccountsController> logger)
    {
        _context = context;
        _passwords = passwords;
        _logger = logger;
    }

    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new InvalidOperationException("Authenticated user has no NameIdentifier claim.");

    private IQueryable<GmailAccount> OwnedAccounts() =>
        _context.GmailAccounts.Where(a => a.OwnerUserId == CurrentUserId);

    private Task<GmailAccount?> FindOwnedAsync(int id, bool tracking = true)
    {
        var query = tracking ? OwnedAccounts() : OwnedAccounts().AsNoTracking();
        return query.FirstOrDefaultAsync(a => a.Id == id);
    }

    // GET: /Accounts?Purpose=&Status=&TwoFactor=yes|no&Search=&Sort=
    public async Task<IActionResult> Index(AccountFilterViewModel filter)
    {
        if (ModelState.GetFieldValidationState(nameof(filter.Purpose)) == ModelValidationState.Invalid)
        {
            ModelState.Remove(nameof(filter.Purpose));
            ModelState.AddModelError(nameof(filter.Purpose), "Unknown purpose - filter ignored.");
            filter.Purpose = null;
        }
        if (ModelState.GetFieldValidationState(nameof(filter.Status)) == ModelValidationState.Invalid)
        {
            ModelState.Remove(nameof(filter.Status));
            ModelState.AddModelError(nameof(filter.Status), "Unknown status - filter ignored.");
            filter.Status = null;
        }
        if (filter.TwoFactor is not (null or "" or "yes" or "no")) filter.TwoFactor = null;

        var query = OwnedAccounts().AsNoTracking();
        if (filter.Purpose.HasValue && Enum.IsDefined(filter.Purpose.Value)) query = query.Where(a => a.Purpose == filter.Purpose);
        if (filter.Status.HasValue && Enum.IsDefined(filter.Status.Value)) query = query.Where(a => a.Status == filter.Status);
        if (filter.TwoFactor == "yes") query = query.Where(a => a.TwoFactorEnabled);
        if (filter.TwoFactor == "no") query = query.Where(a => !a.TwoFactorEnabled);

        var search = filter.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            if (search.Length > 100) search = search[..100];
            var s = search.ToLower();
            query = query.Where(a => a.EmailAddress.ToLower().Contains(s)
                                     || (a.DisplayName != null && a.DisplayName.ToLower().Contains(s))
                                     || (a.Notes != null && a.Notes.ToLower().Contains(s)));
            filter.Search = search;
        }

        if (!AccountSort.Options.Any(o => o.Value == filter.Sort)) filter.Sort = AccountSort.EmailAsc;
        query = filter.Sort switch
        {
            AccountSort.EmailDesc => query.OrderByDescending(a => a.EmailAddress),
            AccountSort.PasswordOldest => query.OrderBy(a => a.LastPasswordChange != null).ThenBy(a => a.LastPasswordChange).ThenBy(a => a.EmailAddress),
            AccountSort.PasswordNewest => query.OrderBy(a => a.LastPasswordChange == null).ThenByDescending(a => a.LastPasswordChange).ThenBy(a => a.EmailAddress),
            AccountSort.CreatedNewest => query.OrderBy(a => a.DateCreated == null).ThenByDescending(a => a.DateCreated).ThenBy(a => a.EmailAddress),
            AccountSort.CreatedOldest => query.OrderBy(a => a.DateCreated == null).ThenBy(a => a.DateCreated).ThenBy(a => a.EmailAddress),
            _ => query.OrderBy(a => a.EmailAddress),
        };

        return View(new AccountIndexViewModel
        {
            Filter = filter,
            Accounts = await query.ToListAsync(),
            IsFiltered = filter.Purpose.HasValue || filter.Status.HasValue || !string.IsNullOrEmpty(filter.TwoFactor) || !string.IsNullOrEmpty(filter.Search),
        });
    }

    // GET: /Accounts/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();
        var account = await FindOwnedAsync(id.Value, tracking: false);
        if (account == null) return NotFound();
        return View(new AccountDetailsViewModel { Account = account, HasPassword = !string.IsNullOrEmpty(account.EncryptedPassword) });
    }

    // POST: /Accounts/Reveal/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Reveal([FromRoute] int id)
    {
        var account = await FindOwnedAsync(id, tracking: false);
        if (account == null) return NotFound();

        var vm = new AccountDetailsViewModel { Account = account, HasPassword = !string.IsNullOrEmpty(account.EncryptedPassword) };
        if (!vm.HasPassword)
            vm.RevealError = "No password is stored for this account.";
        else if (_passwords.TryUnprotect(account.EncryptedPassword, out var plaintext))
            vm.RevealedPassword = plaintext;
        else
            vm.RevealError = "The stored password could not be decrypted. The Data Protection keys in the 'keys' folder " +
                             "may have been deleted or replaced. Restore the keys folder from a backup, or edit the account and enter the password again.";

        return View("Details", vm);
    }

    // GET: /Accounts/Create
    public IActionResult Create() => View(new AccountFormViewModel());

    // POST: /Accounts/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(CreateFields)] AccountFormViewModel vm)
    {
        if (string.IsNullOrEmpty(vm.Password))
            ModelState.AddModelError(nameof(vm.Password), "Password is required.");
        await ValidateUniqueEmailAsync(vm.EmailAddress, null);
        if (!ModelState.IsValid) return View(vm);

        var account = new GmailAccount
        {
            CreatedAt = DateTime.UtcNow,
            OwnerUserId = CurrentUserId,
        };
        Map(vm, account);
        account.EncryptedPassword = _passwords.Protect(vm.Password!);
        account.UpdatedAt = DateTime.UtcNow;

        try
        {
            _context.GmailAccounts.Add(account);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error creating account");
            ModelState.AddModelError(string.Empty, "The account could not be saved. Please try again.");
            return View(vm);
        }

        TempData["Success"] = $"Account {account.EmailAddress} was added.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Accounts/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var account = await FindOwnedAsync(id.Value, tracking: false);
        if (account == null) return NotFound();
        return View(ToViewModel(account));
    }

    // POST: /Accounts/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, [Bind(EditFields)] AccountFormViewModel vm)
    {
        if (id != vm.Id) return NotFound();

        var account = await FindOwnedAsync(id);
        if (account == null) return NotFound();

        vm.IsEdit = true;
        vm.HasStoredPassword = !string.IsNullOrEmpty(account.EncryptedPassword);
        if (string.IsNullOrEmpty(vm.Password) && !vm.HasStoredPassword)
            ModelState.AddModelError(nameof(vm.Password), "Password is required.");
        await ValidateUniqueEmailAsync(vm.EmailAddress, id);
        if (!ModelState.IsValid) return View(vm);

        _context.Entry(account).Property(a => a.Version).OriginalValue = vm.Version;
        var previousPasswordChange = account.LastPasswordChange;
        Map(vm, account);

        if (!string.IsNullOrEmpty(vm.Password))
        {
            account.EncryptedPassword = _passwords.Protect(vm.Password);
            if (account.LastPasswordChange == previousPasswordChange)
                account.LastPasswordChange = DateTime.Today;
        }
        account.UpdatedAt = DateTime.UtcNow;
        account.Version = Guid.NewGuid();

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var current = await FindOwnedAsync(id, tracking: false);
            if (current == null) return NotFound();

            _logger.LogWarning(ex, "Concurrency conflict editing account {Id}", id);
            vm.Version = current.Version;
            ModelState.Remove(nameof(vm.Version));
            ModelState.AddModelError(string.Empty,
                "This account was changed by someone else after you opened it. " +
                "Check the values and click Save again to overwrite them (re-enter the password if you were changing it).");
            return View(vm);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error updating account {Id}", id);
            ModelState.AddModelError(string.Empty, "The changes could not be saved. Please try again.");
            return View(vm);
        }

        TempData["Success"] = $"Account {account.EmailAddress} was updated.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Accounts/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var account = await FindOwnedAsync(id.Value, tracking: false);
        return account == null ? NotFound() : View(account);
    }

    // POST: /Accounts/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
    {
        var account = await FindOwnedAsync(id);
        if (account == null)
        {
            TempData["Success"] = "That account was already deleted.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            _context.GmailAccounts.Remove(account);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error deleting account {Id}", id);
            TempData["Error"] = "The account could not be deleted. Please try again.";
            return RedirectToAction(nameof(Delete), new { id });
        }

        TempData["Success"] = $"Account {account.EmailAddress} was deleted.";
        return RedirectToAction(nameof(Index));
    }

    private async Task ValidateUniqueEmailAsync(string? email, int? excludeId)
    {
        if (string.IsNullOrWhiteSpace(email) || ModelState.GetFieldValidationState(nameof(AccountFormViewModel.EmailAddress)) == ModelValidationState.Invalid)
            return;
        var normalized = Normalize(email)!;
        var ownerId = CurrentUserId;
        if (await OwnedAccounts().AnyAsync(a => a.EmailAddress == normalized && a.Id != excludeId))
            ModelState.AddModelError(nameof(AccountFormViewModel.EmailAddress), "You already have an account with this email address.");
    }

    private static string? Normalize(string? email) =>
        string.IsNullOrWhiteSpace(email) ? null : email.Trim().ToLowerInvariant();

    private static string? Clean(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static void Map(AccountFormViewModel vm, GmailAccount a)
    {
        a.EmailAddress = Normalize(vm.EmailAddress)!;
        a.DisplayName = Clean(vm.DisplayName);
        a.RecoveryEmail = Normalize(vm.RecoveryEmail);
        a.RecoveryPhone = Clean(vm.RecoveryPhone);
        a.TwoFactorEnabled = vm.TwoFactorEnabled;
        a.TwoFactorMethod = vm.TwoFactorEnabled ? vm.TwoFactorMethod!.Value : TwoFactorMethod.None;
        a.Purpose = vm.Purpose!.Value;
        a.Status = vm.Status!.Value;
        a.DateCreated = vm.DateCreated?.Date;
        a.LastPasswordChange = vm.LastPasswordChange?.Date;
        a.LastLogin = vm.LastLogin;
        a.Notes = Clean(vm.Notes);
    }

    private static AccountFormViewModel ToViewModel(GmailAccount a) => new()
    {
        Id = a.Id,
        Version = a.Version,
        IsEdit = true,
        HasStoredPassword = !string.IsNullOrEmpty(a.EncryptedPassword),
        EmailAddress = a.EmailAddress,
        DisplayName = a.DisplayName,
        RecoveryEmail = a.RecoveryEmail,
        RecoveryPhone = a.RecoveryPhone,
        TwoFactorEnabled = a.TwoFactorEnabled,
        TwoFactorMethod = a.TwoFactorMethod,
        Purpose = a.Purpose,
        Status = a.Status,
        DateCreated = a.DateCreated,
        LastPasswordChange = a.LastPasswordChange,
        LastLogin = a.LastLogin,
        Notes = a.Notes,
    };
}
