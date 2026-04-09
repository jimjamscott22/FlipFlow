using FlipFlow.Application.Contracts.Pricing;
using FlipFlow.Domain.Entities;

namespace FlipFlow.Application.Abstractions.Pricing;

public interface IPricingSuggestionService
{
    PricingSuggestionDto GetSuggestion(Item item, DateTimeOffset asOfUtc);
}
