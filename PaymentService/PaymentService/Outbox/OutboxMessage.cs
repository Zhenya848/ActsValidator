using PaymentService.Models.Shared;
using PaymentService.Models.Shared.ValueObjects.Id;

namespace PaymentService.Outbox;

public class OutboxMessage : Entity<OutboxMessageId>
{
    public string Type { get; private set; }
    public string Payload { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public DateTime? ProcessedOn { get; set; }
    public string? Error  { get; set; }
    
    private OutboxMessage(OutboxMessageId id) : base(id)
    {
        
    }
    
    public OutboxMessage(
        OutboxMessageId id, 
        string type, 
        string payload,
        DateTime occurredOn, 
        DateTime? processedOn = null, 
        string? error = null) : base(id)
    {
        Type = type;
        Payload = payload;
        OccurredOn = occurredOn;
        ProcessedOn = processedOn;
        Error = error;
    }
}