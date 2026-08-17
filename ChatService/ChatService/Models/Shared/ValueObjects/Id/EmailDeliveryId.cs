namespace ChatService.Models.Shared.ValueObjects.Id;

public record EmailDeliveryId
{
    public Guid Value { get; }
    
    public EmailDeliveryId(Guid value) => Value = value;
    
    public static EmailDeliveryId AddNewId() => new (Guid.NewGuid());
    
    public static EmailDeliveryId AddEmptyId() => new (Guid.Empty);
    
    public static EmailDeliveryId Create(Guid id) => new (id);

    public static implicit operator Guid(EmailDeliveryId id)
    {
        ArgumentNullException.ThrowIfNull(id);
        
        return id.Value;
    }
}