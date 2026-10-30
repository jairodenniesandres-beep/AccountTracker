using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AccountTracker.Models;

namespace AccountTracker.Data;

public class AppDbContext : IdentityDbContext<IdentityUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<GmailAccount> GmailAccounts => Set<GmailAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<GmailAccount>(e =>
        {
            e.Property(x => x.EmailAddress).HasMaxLength(254).IsRequired();
            e.Property(x => x.OwnerUserId).HasMaxLength(450).IsRequired();
            e.HasIndex(x => new { x.OwnerUserId, x.EmailAddress }).IsUnique();
            e.HasIndex(x => x.OwnerUserId);
            e.HasOne(x => x.Owner)
                .WithMany()
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(x => x.DisplayName).HasMaxLength(100);
            e.Property(x => x.EncryptedPassword).HasMaxLength(2000);
            e.Property(x => x.RecoveryEmail).HasMaxLength(254);
            e.Property(x => x.RecoveryPhone).HasMaxLength(30);
            e.Property(x => x.Notes).HasMaxLength(1000);
            e.Property(x => x.TwoFactorMethod).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Purpose).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
        });
    }
}
