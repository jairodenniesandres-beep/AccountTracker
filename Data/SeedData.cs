using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;

namespace AccountTracker.Data;

public static class SeedData
{
    private static readonly string[] SampleEmails =
    {
        "example.personal.demo@gmail.com",
        "example.work.demo@gmail.com",
        "example.school.demo@googlemail.com",
        "example.business.demo@gmail.com",
        "example.testing.demo@gmail.com"
    };

    // Previously seeded logins — removed on startup so Register is the way in.
    private static readonly string[] LegacySeedEmails =
    {
        "user@aptech.com",
        "dennies@gmail.com",
        "admin@aptech.com",
        "admin@local.test",
        "user@local.test"
    };

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();
        await ClearSampleAccountsAsync(context);
        await ClearLegacySeedUsersAsync(scope.ServiceProvider);
    }

    private static async Task ClearSampleAccountsAsync(AppDbContext context)
    {
        var hadSample = await context.GmailAccounts.AnyAsync(a =>
            SampleEmails.Contains(a.EmailAddress)
            || (a.Notes != null && a.Notes.Contains("SAMPLE DATA")));
        if (hadSample)
            await context.GmailAccounts.ExecuteDeleteAsync();
    }

    private static async Task ClearLegacySeedUsersAsync(IServiceProvider services)
    {
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        foreach (var email in LegacySeedEmails)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user != null)
                await userManager.DeleteAsync(user);
        }

        if (await roleManager.RoleExistsAsync("Admin"))
        {
            var adminRole = await roleManager.FindByNameAsync("Admin");
            if (adminRole != null)
                await roleManager.DeleteAsync(adminRole);
        }
    }
}
