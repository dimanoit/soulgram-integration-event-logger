namespace Soulgram.EventLogger;

public class CompressedIntegrationEvent
{
    public string EventName { get; set; }
    public byte[] Content { get; set; }
}