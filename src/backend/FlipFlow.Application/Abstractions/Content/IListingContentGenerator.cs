using FlipFlow.Application.Contracts.Content;
using FlipFlow.Domain.Entities;

namespace FlipFlow.Application.Abstractions.Content;

public interface IListingContentGenerator
{
    Task<GeneratedListingContentDto> GenerateAsync(Item item, CancellationToken cancellationToken = default);
}
