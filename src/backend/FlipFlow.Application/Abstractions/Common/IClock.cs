namespace FlipFlow.Application.Abstractions.Common;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
