using FlipFlow.Application.Abstractions.Auth;
using FlipFlow.Application.Abstractions.Items;
using FlipFlow.Application.Contracts.Items;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FlipFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/items")]
public sealed class ItemsController(
    ICurrentUserService currentUserService,
    IItemService itemService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ItemSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ItemSummaryDto>>> GetItems(CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        var items = await itemService.GetItemsAsync(userId, cancellationToken);

        return Ok(items);
    }

    [HttpGet("{itemId:guid}")]
    [ProducesResponseType(typeof(ItemDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDetailDto>> GetItem(Guid itemId, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        var item = await itemService.GetItemAsync(userId, itemId, cancellationToken);

        return Ok(item);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ItemDetailDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ItemDetailDto>> CreateItem(
        [FromBody] SaveItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        var createdItem = await itemService.CreateItemAsync(userId, request, cancellationToken);

        return CreatedAtAction(nameof(GetItem), new { itemId = createdItem.Id }, createdItem);
    }

    [HttpPut("{itemId:guid}")]
    [ProducesResponseType(typeof(ItemDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemDetailDto>> UpdateItem(
        Guid itemId,
        [FromBody] SaveItemRequestDto request,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        var updatedItem = await itemService.UpdateItemAsync(userId, itemId, request, cancellationToken);

        return Ok(updatedItem);
    }

    [HttpDelete("{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(Guid itemId, CancellationToken cancellationToken)
    {
        var userId = currentUserService.GetRequiredUserId();
        await itemService.DeleteItemAsync(userId, itemId, cancellationToken);

        return NoContent();
    }

    [HttpPost("{itemId:guid}/photos")]
    [ProducesResponseType(typeof(ItemPhotoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ItemPhotoDto>> UploadPhoto(
        Guid itemId,
        [FromForm] IFormFile? file,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
        {
            throw new ValidationException([
                new FluentValidation.Results.ValidationFailure("file", "Select an image to upload.")
            ]);
        }

        if (!ItemPhotoUploadRules.AllowedContentTypes.Contains(file.ContentType))
        {
            throw new ValidationException([
                new FluentValidation.Results.ValidationFailure("file", "Only JPG, PNG, and WEBP images are supported.")
            ]);
        }

        if (file.Length > ItemPhotoUploadRules.MaxFileSizeBytes)
        {
            throw new ValidationException([
                new FluentValidation.Results.ValidationFailure("file", "Images must be 5 MB or smaller.")
            ]);
        }

        await using var fileStream = file.OpenReadStream();

        var userId = currentUserService.GetRequiredUserId();
        var photo = await itemService.AddPhotoAsync(
            userId,
            itemId,
            fileStream,
            file.FileName,
            file.ContentType,
            cancellationToken);

        return CreatedAtAction(nameof(GetItem), new { itemId }, photo);
    }
}
