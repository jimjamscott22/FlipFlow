using FlipFlow.Application.Contracts.Items;

namespace FlipFlow.Application.Abstractions.Items;

public interface IItemService
{
    Task<IReadOnlyList<ItemSummaryDto>> GetItemsAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<ItemDetailDto> GetItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);

    Task<ItemDetailDto> CreateItemAsync(Guid userId, SaveItemRequestDto request, CancellationToken cancellationToken = default);

    Task<ItemDetailDto> UpdateItemAsync(Guid userId, Guid itemId, SaveItemRequestDto request, CancellationToken cancellationToken = default);

    Task DeleteItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default);

    Task<ItemPhotoDto> AddPhotoAsync(
        Guid userId,
        Guid itemId,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
