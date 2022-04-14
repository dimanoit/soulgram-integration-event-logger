using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Soulgram.EventLogger;

public class IntegrationEventLogService : IIntegrationEventLogService
{
    private readonly IntegrationEventLogContext _integrationEventLogContext;

    public IntegrationEventLogService(IntegrationEventLogContext integrationEventLogContext)
    {
        _integrationEventLogContext = integrationEventLogContext;
    }

    public async Task<IEnumerable<CompressedIntegrationEvent>> GetPublishedFailedEvents(
        CancellationToken cancellationToken)
    {
        var eventLogs = await _integrationEventLogContext
            .IntegrationEventLogs
            .AsNoTracking()
            .Where(x => x.State == EventStateEnum.PublishedFailed)
            .Select(x => x.ToCompressedEvent())
            .ToArrayAsync(cancellationToken);

        return eventLogs;
    }

    public Task SaveEventAsync(IntegrationEventLogEntry @event, IDbContextTransaction transaction)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        _integrationEventLogContext.Database.UseTransaction(transaction.GetDbTransaction());
        _integrationEventLogContext.IntegrationEventLogs.Add(@event);

        return _integrationEventLogContext.SaveChangesAsync();
    }

    public Task MarkEventAsPublishedAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.Published);
    }

    public Task MarkEventAsInProgressAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.InProgress);
    }

    public Task MarkEventAsFailedAsync(Guid eventId)
    {
        return UpdateEventStatus(eventId, EventStateEnum.PublishedFailed);
    }

    private Task UpdateEventStatus(Guid eventId, EventStateEnum status)
    {
        var eventLogEntry = _integrationEventLogContext
            .IntegrationEventLogs
            .Single(ie => ie.EventId == eventId);

        eventLogEntry.State = status;

        if (status == EventStateEnum.InProgress)
        {
            eventLogEntry.TimesSent++;
        }

        _integrationEventLogContext.IntegrationEventLogs.Update(eventLogEntry);

        return _integrationEventLogContext.SaveChangesAsync();
    }
}