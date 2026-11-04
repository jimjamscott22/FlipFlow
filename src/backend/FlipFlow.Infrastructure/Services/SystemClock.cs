using FlipFlow.Application.Abstractions.Common;

namespace FlipFlow.Infrastructure.Services;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
