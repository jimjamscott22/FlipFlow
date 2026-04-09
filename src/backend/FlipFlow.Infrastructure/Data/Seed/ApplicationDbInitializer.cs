using FlipFlow.Domain.Entities;
using FlipFlow.Domain.Enums;
using FlipFlow.Infrastructure.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlipFlow.Infrastructure.Data.Seed;

public sealed class ApplicationDbInitializer(
    AppDbContext dbContext,
    RoleManager<IdentityRole<Guid>> roleManager,
    UserManager<AppUser> userManager,
    ILogger<ApplicationDbInitializer> logger)
{
    private static readonly string[] Roles = ["User"];

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
            }
        }

        const string demoEmail = "demo@flipflow.local";
        const string demoPassword = "FlipFlow123";

        var demoUser = await userManager.FindByEmailAsync(demoEmail);

        if (demoUser is null)
        {
            demoUser = new AppUser
            {
                Email = demoEmail,
                UserName = demoEmail,
                DisplayName = "Demo Seller"
            };

            var creationResult = await userManager.CreateAsync(demoUser, demoPassword);

            if (!creationResult.Succeeded)
            {
                logger.LogWarning("Demo user could not be created: {Errors}", string.Join(", ", creationResult.Errors.Select(x => x.Description)));
                return;
            }

            await userManager.AddToRoleAsync(demoUser, "User");
        }

        if (!await dbContext.Items.AnyAsync(cancellationToken))
        {
            dbContext.Items.Add(new Item
            {
                OwnerUserId = demoUser.Id,
                Title = "Nintendo Switch OLED",
                Brand = "Nintendo",
                Model = "HEG-001",
                Category = "Electronics",
                Condition = ItemCondition.Good,
                Description = "Well-kept console with dock, charger, and Joy-Cons.",
                AskingPrice = 259.99m,
                Status = ItemStatus.Draft
            });

            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
