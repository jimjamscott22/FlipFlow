using FlipFlow.Domain.Entities;
using FlipFlow.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FlipFlow.Infrastructure.Data.Seed;

/// <summary>Applies migrations on startup and, if enabled, adds sample data to an empty database.</summary>
public sealed class ApplicationDbInitializer(
    AppDbContext dbContext,
    IOptions<DemoDataOptions> demoOptions,
    ILogger<ApplicationDbInitializer> logger)
{
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (!demoOptions.Value.Enabled || await dbContext.Items.AnyAsync(cancellationToken))
        {
            return;
        }

        logger.LogInformation("Seeding demo items.");

        dbContext.Items.AddRange(
            new Item
            {
                Title = "Nintendo Switch OLED",
                Brand = "Nintendo",
                Model = "HEG-001",
                Category = "Electronics",
                Condition = ItemCondition.Good,
                Description = "Well-kept console with dock, charger, and Joy-Cons.",
                AskingPrice = 259.99m,
                Status = ItemStatus.Draft
            },
            new Item
            {
                Title = "Cast iron skillet, 12 inch",
                Brand = "Lodge",
                Model = "L10SK3",
                Category = "Home & Kitchen",
                Condition = ItemCondition.LikeNew,
                Description = "Seasoned, no rust, barely used.",
                AskingPrice = 28m,
                Status = ItemStatus.ReadyToList
            });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
