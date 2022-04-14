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

    public async Task<IEnumerable<IntegrationEventLogEntry>> GetPublishedFailedLogs(CancellationToken cancellationToken)
    {
        var eventLogs = await _integrationEventLogContext
            .IntegrationEventLogs
            .AsNoTracking()
            .Where(x => x.State == EventStateEnum.PublishedFailed)
            .ToArrayAsync(cancellationToken);

        return eventLogs;
    }

    public Task SaveEventAsync(IntegrationEvent @event, IDbContextTransaction transaction)
    {
        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        var eventLogEntry = @event.ToIntegrationEventLogEntry(transaction.TransactionId);

        _integrationEventLogContext.Database.UseTransaction(transaction.GetDbTransaction());
        _integrationEventLogContext.IntegrationEventLogs.Add(eventLogEntry);

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