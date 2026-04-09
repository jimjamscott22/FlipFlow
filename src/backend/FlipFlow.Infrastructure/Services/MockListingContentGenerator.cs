using FlipFlow.Application.Abstractions.Content;
using FlipFlow.Application.Contracts.Content;
using FlipFlow.Domain.Entities;

namespace FlipFlow.Infrastructure.Services;

public sealed class MockListingContentGenerator : IListingContentGenerator
{
    public Task<GeneratedListingContentDto> GenerateAsync(Item item, CancellationToken cancellationToken = default)
    {
        var title = $"{item.Brand} {item.Model} {item.Condition} - {item.Title}".Trim();
        var description = string.Join(
            Environment.NewLine,
            [
                $"{item.Brand} {item.Model}",
                $"Category: {item.Category}",
                $"Condition: {item.Condition}",
                item.Description,
                "Includes what is shown in the photos."
            ]);

        return Task.FromResult(new GeneratedListingContentDto
        {
            Title = title,
            Description = description
        });
    }
}
