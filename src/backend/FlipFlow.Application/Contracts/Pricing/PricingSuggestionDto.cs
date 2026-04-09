namespace FlipFlow.Application.Contracts.Pricing;

public sealed class PricingSuggestionDto
{
    public decimal SuggestedPrice { get; set; }

    public string Reason { get; set; } = string.Empty;
}
