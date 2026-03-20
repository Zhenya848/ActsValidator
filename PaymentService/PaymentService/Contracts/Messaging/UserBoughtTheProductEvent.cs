namespace PaymentService.Contracts.Messaging;

public class UserBoughtTheProductEvent
{
    public Guid UserId { get; set; }
    public int Pack {  get; set; }
}