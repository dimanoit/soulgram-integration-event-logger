namespace Soulgram.EventLogger;

public class CompressedIntegrationEvent
{
    public Guid EventId { get; set; }
    public string EventName { get; set; }
    public byte[] Content { get; set; }
}