using System.Text;

namespace Soulgram.EventLogger;

public static class IntegrationEventConverter
{
    public static CompressedIntegrationEvent ToCompressedEvent(this IntegrationEventLogEntry entry)
    {
        var @event = new CompressedIntegrationEvent
        {
            Content = Encoding.ASCII.GetBytes(entry.Content),
            EventName = entry.EventName,
            EventId = entry.EventId
        };

        return @event;
    }
}