using Microsoft.EntityFrameworkCore.Storage;

namespace Soulgram.EventLogger;

public interface IIntegrationEventLogService
{
    Task<IEnumerable<IntegrationEventLogEntry>> GetPublishedFailedLogs(CancellationToken cancellationToken);
    Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction);

    Task MarkEventAsPublishedAsync(Guid eventId);
    Task MarkEventAsInProgressAsync(Guid eventId);
    Task MarkEventAsFailedAsync(Guid eventId);
}