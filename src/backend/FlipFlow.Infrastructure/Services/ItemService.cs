using FlipFlow.Application.Abstractions.Common;
using FlipFlow.Application.Abstractions.Items;
using FlipFlow.Application.Contracts.Items;
using FlipFlow.Domain.Entities;
using FlipFlow.Infrastructure.Data;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FlipFlow.Infrastructure.Services;

public sealed class ItemService(
    AppDbContext dbContext,
    IFileStorageService fileStorageService,
    ILogger<ItemService> logger) : IItemService
{
    public async Task<IReadOnlyList<ItemSummaryDto>> GetItemsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var items = await dbContext.Items
            .AsNoTracking()
            .Include(x => x.Photos)
            .Where(x => x.OwnerUserId == userId)
            .OrderByDescending(x => x.UpdatedAtUtc)
            .ToListAsync(cancellationToken);

        return items.Select(MapSummary).ToList();
    }

    public async Task<ItemDetailDto> GetItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await GetOwnedItemAsync(userId, itemId, cancellationToken);
        return MapDetail(item);
    }

    public async Task<ItemDetailDto> CreateItemAsync(Guid userId, SaveItemRequestDto request, CancellationToken cancellationToken = default)
    {
        var item = new Item
        {
            OwnerUserId = userId
        };

        Apply(item, request);

        dbContext.Items.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapDetail(item);
    }

    public async Task<ItemDetailDto> UpdateItemAsync(
        Guid userId,
        Guid itemId,
        SaveItemRequestDto request,
        CancellationToken cancellationToken = default)
    {
        var item = await GetOwnedItemAsync(userId, itemId, cancellationToken);

        Apply(item, request);

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapDetail(item);
    }

    public async Task DeleteItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken = default)
    {
        var item = await GetOwnedItemAsync(userId, itemId, cancellationToken);
        var photoPaths = item.Photos.Select(x => x.RelativePath).ToList();

        dbContext.Items.Remove(item);
        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var photoPath in photoPaths)
        {
            try
            {
                await fileStorageService.DeleteFileAsync(photoPath, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Unable to delete stored item photo {PhotoPath}", photoPath);
            }
        }
    }

    public async Task<ItemPhotoDto> AddPhotoAsync(
        Guid userId,
        Guid itemId,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var item = await GetOwnedItemAsync(userId, itemId, cancellationToken);

        if (item.Photos.Count >= ItemPhotoUploadRules.MaxPhotosPerItem)
        {
            throw new ValidationException([
                new FluentValidation.Results.ValidationFailure("photos", $"You can upload up to {ItemPhotoUploadRules.MaxPhotosPerItem} photos per item.")
            ]);
        }

        var storedFile = await fileStorageService.SaveFileAsync(fileStream, fileName, contentType, cancellationToken);

        var photo = new ItemPhoto
        {
            ItemId = item.Id,
            FileName = storedFile.FileName,
            StoredFileName = storedFile.StoredFileName,
            RelativePath = storedFile.RelativePath,
            ContentType = storedFile.ContentType,
            FileSizeBytes = storedFile.FileSizeBytes,
            SortOrder = item.Photos.Count == 0 ? 1 : item.Photos.Max(x => x.SortOrder) + 1,
            IsPrimary = item.Photos.Count == 0
        };

        item.UpdatedAtUtc = DateTimeOffset.UtcNow;
        item.Photos.Add(photo);

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapPhoto(photo);
    }

    private async Task<Item> GetOwnedItemAsync(Guid userId, Guid itemId, CancellationToken cancellationToken)
    {
        var item = await dbContext.Items
            .Include(x => x.Photos)
            .FirstOrDefaultAsync(x => x.OwnerUserId == userId && x.Id == itemId, cancellationToken);

        return item ?? throw new KeyNotFoundException("The requested item could not be found.");
    }

    private static void Apply(Item item, SaveItemRequestDto request)
    {
        item.Title = request.Title.Trim();
        item.Brand = request.Brand.Trim();
        item.Model = request.Model.Trim();
        item.Category = request.Category.Trim();
        item.Condition = request.Condition;
        item.Description = request.Description.Trim();
        item.AskingPrice = decimal.Round(request.AskingPrice, 2, MidpointRounding.AwayFromZero);
        item.Status = request.Status;
    }

    private static ItemSummaryDto MapSummary(Item item)
    {
        var orderedPhotos = OrderPhotos(item.Photos);
        var primaryPhoto = orderedPhotos.FirstOrDefault(x => x.IsPrimary) ?? orderedPhotos.FirstOrDefault();

        return new ItemSummaryDto
        {
            Id = item.Id,
            Title = item.Title,
            Brand = item.Brand,
            Model = item.Model,
            Category = item.Category,
            Condition = item.Condition,
            AskingPrice = item.AskingPrice,
            Status = item.Status,
            PhotoCount = orderedPhotos.Count,
            PrimaryPhotoRelativePath = primaryPhoto?.RelativePath,
            UpdatedAtUtc = item.UpdatedAtUtc
        };
    }

    private static ItemDetailDto MapDetail(Item item)
    {
        return new ItemDetailDto
        {
            Id = item.Id,
            Title = item.Title,
            Brand = item.Brand,
            Model = item.Model,
            Category = item.Category,
            Condition = item.Condition,
            Description = item.Description,
            AskingPrice = item.AskingPrice,
            Status = item.Status,
            CreatedAtUtc = item.CreatedAtUtc,
            UpdatedAtUtc = item.UpdatedAtUtc,
            Photos = OrderPhotos(item.Photos).Select(MapPhoto).ToList()
        };
    }

    private static ItemPhotoDto MapPhoto(ItemPhoto photo)
    {
        return new ItemPhotoDto
        {
            Id = photo.Id,
            FileName = photo.FileName,
            ContentType = photo.ContentType,
            FileSizeBytes = photo.FileSizeBytes,
            SortOrder = photo.SortOrder,
            IsPrimary = photo.IsPrimary,
            RelativePath = photo.RelativePath,
            CreatedAtUtc = photo.CreatedAtUtc
        };
    }

    private static List<ItemPhoto> OrderPhotos(IEnumerable<ItemPhoto> photos)
    {
        return photos
            .OrderByDescending(x => x.IsPrimary)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .ToList();
    }
}
