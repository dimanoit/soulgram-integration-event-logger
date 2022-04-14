using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Soulgram.EventLogger;

public class IntegrationEventLogService : IIntegrationEventLogService, IDisposable
{
    private readonly IntegrationEventLogContext _integrationEventLogContext;
    private volatile bool _disposedValue;

    public IntegrationEventLogService(DbConnection dbConnection)
    {
        var connection = dbConnection ?? throw new ArgumentNullException(nameof(dbConnection));

        var dbContextOptions = new DbContextOptionsBuilder<IntegrationEventLogContext>()
            .UseSqlServer(connection)
            .Options;

        _integrationEventLogContext = new IntegrationEventLogContext(dbContextOptions);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
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

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _integrationEventLogContext?.Dispose();
            }


            _disposedValue = true;
        }
    }
}