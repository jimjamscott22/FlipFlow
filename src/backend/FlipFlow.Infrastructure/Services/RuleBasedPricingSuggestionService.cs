using FlipFlow.Application.Abstractions.Pricing;
using FlipFlow.Application.Contracts.Pricing;
using FlipFlow.Domain.Entities;
using FlipFlow.Domain.Enums;

namespace FlipFlow.Infrastructure.Services;

public sealed class RuleBasedPricingSuggestionService : IPricingSuggestionService
{
    public PricingSuggestionDto GetSuggestion(Item item, DateTimeOffset asOfUtc)
    {
        var multiplier = item.Condition switch
        {
            ItemCondition.New => 1.00m,
            ItemCondition.LikeNew => 0.95m,
            ItemCondition.Good => 0.88m,
            ItemCondition.Fair => 0.78m,
            ItemCondition.Poor => 0.65m,
            ItemCondition.ForParts => 0.40m,
            _ => 0.85m
        };

        if (!string.Equals(item.Category, "Electronics", StringComparison.OrdinalIgnoreCase))
        {
            multiplier -= 0.03m;
        }

        var daysSinceCreated = Math.Max(0, (asOfUtc - item.CreatedAtUtc).Days);
        var ageAdjustment = daysSinceCreated >= 30 ? 0.92m : 1.00m;
        var suggestedPrice = Math.Round(item.AskingPrice * multiplier * ageAdjustment, 2);

        return new PricingSuggestionDto
        {
            SuggestedPrice = suggestedPrice,
            Reason = daysSinceCreated >= 30
                ? "Item has been unsold for at least 30 days, so a modest discount is recommended."
                : "Suggested price is based on item condition and current category defaults."
        };
    }
}
