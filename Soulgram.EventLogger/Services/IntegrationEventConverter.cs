using System.Text;
using System.Text.Json;

namespace Soulgram.EventLogger;

public static class IntegrationEventConverter
{
    public static CompressedIntegrationEvent ToCompressedEvent(this IntegrationEventLogEntry entry)
    {
        var @event = new CompressedIntegrationEvent
        {
            Content = Encoding.ASCII.GetBytes(entry.Content),
            EventName = entry.EventName
        };

        return @event;
    }

    public static IntegrationEventLogEntry ToIntegrationEventLogEntry(
        this IntegrationEvent @event,
        Guid transactionId)
    {
        var entry = new IntegrationEventLogEntry
        {
            EventId = @event.Id,
            CreationTime = @event.CreationDate,
            EventName = @event.GetType().Name,
            TransactionId = transactionId.ToString(),

            State = EventStateEnum.NotPublished,
            TimesSent = 0,
            Content = ToByteString(@event)
        };

        return entry;
    }

    private static string ToByteString(IntegrationEvent @event)
    {
        var bytes = JsonSerializer.SerializeToUtf8Bytes(
            @event,
            @event.GetType(),
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        var str = Encoding.ASCII.GetString(bytes);

        return str;
    }
}