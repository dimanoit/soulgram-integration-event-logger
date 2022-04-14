namespace Soulgram.EventLogger;

public class IntegrationEventLogEntry
{
    public Guid EventId { get; set; }
    public EventStateEnum State { get; set; }
    public string EventName { get; set; }
    public int TimesSent { get; set; }
    public DateTime CreationTime { get; set; }
    public string Content { get; set; }
    public string TransactionId { get; set; }
}