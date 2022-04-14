using Microsoft.EntityFrameworkCore.Storage;

namespace Soulgram.EventLogger;

public interface IIntegrationEventLogService
{
    Task<IEnumerable<CompressedIntegrationEvent>> GetPublishedFailedEvents(CancellationToken cancellationToken);
    Task SaveEventAsync(IntegrationEventLogEntry @event, IDbContextTransaction transaction);

    Task MarkEventAsPublishedAsync(Guid eventId);
    Task MarkEventAsInProgressAsync(Guid eventId);
    Task MarkEventAsFailedAsync(Guid eventId);
}