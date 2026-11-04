using FlipFlow.Application.Contracts.Items;

namespace FlipFlow.Application.Abstractions.Items;

public interface IItemService
{
    Task<IReadOnlyList<ItemSummaryDto>> GetItemsAsync(CancellationToken cancellationToken = default);

    Task<ItemDetailDto> GetItemAsync(Guid itemId, CancellationToken cancellationToken = default);

    Task<ItemDetailDto> CreateItemAsync(SaveItemRequestDto request, CancellationToken cancellationToken = default);

    Task<ItemDetailDto> UpdateItemAsync(Guid itemId, SaveItemRequestDto request, CancellationToken cancellationToken = default);

    Task DeleteItemAsync(Guid itemId, CancellationToken cancellationToken = default);

    Task<ItemPhotoDto> AddPhotoAsync(
        Guid itemId,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
