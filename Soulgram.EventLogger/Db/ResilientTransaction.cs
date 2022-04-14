using Microsoft.EntityFrameworkCore;

namespace Soulgram.EventLogger;

public class ResilientTransaction : IResilientTransaction
{
    private readonly DbContext _context;

    ResilientTransaction(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken)
    {
        //Use of an EF Core resiliency strategy when using multiple DbContexts within an explicit BeginTransaction():
        //See: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using (var transaction = await _context.Database.BeginTransactionAsync(cancellationToken))
            {
                await action();
                await transaction.CommitAsync(cancellationToken);
            }
        });
    }
}