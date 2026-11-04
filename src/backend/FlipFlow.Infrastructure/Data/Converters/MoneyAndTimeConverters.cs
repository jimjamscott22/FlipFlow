using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FlipFlow.Infrastructure.Data.Converters;

/// <summary>Stores a decimal amount of money as whole cents (long).</summary>
public sealed class DecimalToCentsConverter() : ValueConverter<decimal, long>(
    value => (long)decimal.Round(value * 100m, 0, MidpointRounding.AwayFromZero),
    cents => cents / 100m);

public sealed class NullableDecimalToCentsConverter() : ValueConverter<decimal?, long?>(
    value => value.HasValue ? (long)decimal.Round(value.Value * 100m, 0, MidpointRounding.AwayFromZero) : null,
    cents => cents.HasValue ? cents.Value / 100m : null);

/// <summary>Stores a DateTimeOffset as UTC ticks so it sorts and compares correctly in SQLite.</summary>
public sealed class DateTimeOffsetToTicksConverter() : ValueConverter<DateTimeOffset, long>(
    value => value.UtcTicks,
    ticks => new DateTimeOffset(ticks, TimeSpan.Zero));

public sealed class NullableDateTimeOffsetToTicksConverter() : ValueConverter<DateTimeOffset?, long?>(
    value => value.HasValue ? value.Value.UtcTicks : null,
    ticks => ticks.HasValue ? new DateTimeOffset(ticks.Value, TimeSpan.Zero) : null);
