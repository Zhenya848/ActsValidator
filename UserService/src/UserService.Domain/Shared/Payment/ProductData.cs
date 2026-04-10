namespace UserService.Domain.Shared.Payment;

public record ProductData
{
    public string ProductId { get; set; }
    public int Amount { get; set; }
    public int Months { get; set; }
}