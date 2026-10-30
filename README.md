# AccountTracker

Private Gmail account / password vault - ASP.NET Core MVC with EF Core, SQLite, and Identity auth.

- .NET 10 (LTS) - `net10.0`, built with SDK 10.0.402
- Microsoft.EntityFrameworkCore.Sqlite 10.0.13
- Microsoft.AspNetCore.Identity.EntityFrameworkCore 10.0.13

## Run
    dotnet restore
    dotnet run

Open the app and **Register** a new account at `/Identity/Account/Register`, then **Login** at `/Identity/Account/Login`. There are **no pre-seeded users** — create your own. Vaults start empty.

If an older database still has demo logins (`user@aptech.com`, `dennies@gmail.com`, `admin@aptech.com`, etc.), startup removes those leftover seed accounts (and any leftover sample Gmail rows). To start completely clean you can also delete `AccountTracker.db` (and keep a backup of `keys/` if you still need encrypted passwords) and re-run.

## Per-user vault
**Each logged-in user only sees and manages their own `GmailAccount` rows** (`OwnerUserId`). Index / Details / Create / Edit / Delete / Reveal all filter by the current user. Accessing another user's account id returns **404**.

## What it does
- Per-user CRUD for Gmail accounts (email, display name, encrypted password, recovery info, 2FA, purpose, status, dates, notes).
- Index filters: purpose, status, 2FA yes/no, search, sorting (scoped to your rows).
- Passwords are encrypted and only shown after **Reveal** on Details (never on lists).

## Security - please read
**Meant for local personal use.** Cookie auth protects pages; don't host on a public network without hardening.

- **Passwords are encrypted** with ASP.NET Core Data Protection (`AccountTracker.GmailAccount.Password.v1`).
- **Encryption keys are in `./keys`** — back up with the database. Lost keys = undecryptable passwords.
- **Never commit `AccountTracker.db` or `keys/`**.
- Notes / recovery fields are **not** encrypted.
- Isolation is by `OwnerUserId` FK to `AspNetUsers`.

## Changing the model later
    dotnet ef migrations add <Name>

## Notes
- Styling uses Bootstrap 5 from `wwwroot/lib`.
