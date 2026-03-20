namespace PaymentService.Models.Shared.ValueObjects;

public record ProductData
{
    public string ProductId { get; set; }
    public int Price { get; set; }
    public int Pack { get; set; }
}