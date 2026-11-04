using FlipFlow.Application.Abstractions.Common;
using FlipFlow.Application.Contracts.Items;
using FlipFlow.Domain.Entities;
using FlipFlow.Domain.Enums;
using FlipFlow.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace FlipFlow.Tests;

public sealed class ItemServiceTests : IClassFixture<SqliteDbFixture>
{
    private readonly SqliteDbFixture _db;

    public ItemServiceTests(SqliteDbFixture db) => _db = db;

    private ItemService CreateService() =>
        new(_db.CreateContext(), new NoopFileStorage(), NullLogger<ItemService>.Instance);

    private static SaveItemRequestDto Request(string title, decimal price = 25.5m) => new()
    {
        Title = title,
        Brand = "IKEA",
        Model = "X1",
        Category = "Home",
        Condition = ItemCondition.Good,
        Description = "  a lamp  ",
        AskingPrice = price,
        Status = ItemStatus.Draft
    };

    [Fact]
    public async Task Create_then_read_round_trips_the_item()
    {
        var created = await CreateService().CreateItemAsync(Request("Round trip lamp"));

        var read = await CreateService().GetItemAsync(created.Id);

        Assert.Equal("Round trip lamp", read.Title);
        Assert.Equal("a lamp", read.Description);
        Assert.Equal(25.5m, read.AskingPrice);
        Assert.Equal(ItemStatus.Draft, read.Status);
        Assert.Empty(read.Photos);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(19.99)]
    [InlineData(1234567.89)]
    public async Task Money_survives_storage_exactly(double price)
    {
        var amount = (decimal)price;
        var created = await CreateService().CreateItemAsync(Request($"Price {amount}", amount));

        var read = await CreateService().GetItemAsync(created.Id);

        Assert.Equal(amount, read.AskingPrice);
    }

    [Fact]
    public async Task List_orders_by_most_recently_updated_first()
    {
        var older = await CreateService().CreateItemAsync(Request("Older item"));
        await Task.Delay(20);
        var newer = await CreateService().CreateItemAsync(Request("Newer item"));
        await Task.Delay(20);
        await CreateService().UpdateItemAsync(older.Id, Request("Older item, edited"));

        var items = await CreateService().GetItemsAsync();
        var ids = items.Select(x => x.Id).ToList();

        Assert.True(ids.IndexOf(older.Id) < ids.IndexOf(newer.Id));
    }

    [Fact]
    public async Task Adding_a_photo_inserts_a_row_and_marks_the_first_as_primary()
    {
        var created = await CreateService().CreateItemAsync(Request("Photo item"));

        using var stream = new MemoryStream([1, 2, 3]);
        var first = await CreateService().AddPhotoAsync(created.Id, stream, "a.png", "image/png");
        using var stream2 = new MemoryStream([4, 5]);
        var second = await CreateService().AddPhotoAsync(created.Id, stream2, "b.png", "image/png");

        Assert.True(first.IsPrimary);
        Assert.False(second.IsPrimary);
        Assert.Equal(2, await _db.CreateContext().ItemPhotos.CountAsync(x => x.ItemId == created.Id));
    }

    [Fact]
    public async Task Deleting_an_item_removes_its_photos()
    {
        var created = await CreateService().CreateItemAsync(Request("Delete me"));
        using var stream = new MemoryStream([1]);
        await CreateService().AddPhotoAsync(created.Id, stream, "a.png", "image/png");

        await CreateService().DeleteItemAsync(created.Id);

        Assert.False(await _db.CreateContext().Items.AnyAsync(x => x.Id == created.Id));
        Assert.False(await _db.CreateContext().ItemPhotos.AnyAsync(x => x.ItemId == created.Id));
    }

    [Fact]
    public async Task Reading_a_missing_item_throws_not_found()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(() => CreateService().GetItemAsync(Guid.NewGuid()));
    }

    private sealed class NoopFileStorage : IFileStorageService
    {
        public Task<StoredFileResult> SaveFileAsync(Stream fileStream, string originalFileName, string contentType, CancellationToken cancellationToken = default)
        {
            var stored = $"{Guid.NewGuid():N}.png";
            return Task.FromResult(new StoredFileResult(originalFileName, stored, $"/storage/uploads/{stored}", contentType, fileStream.Length));
        }

        public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
