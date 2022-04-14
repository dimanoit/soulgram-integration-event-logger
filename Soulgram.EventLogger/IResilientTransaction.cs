namespace Soulgram.EventLogger;

public interface IResilientTransaction
{
    Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);
}