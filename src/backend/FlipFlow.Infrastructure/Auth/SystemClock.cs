using FlipFlow.Application.Abstractions.Common;

namespace FlipFlow.Infrastructure.Auth;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
