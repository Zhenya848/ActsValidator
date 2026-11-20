namespace PaymentService.Providers.MyTax.Responces;

public record SendReceiptResult
{
    public string ReceiptId { get; init; }
    public Uri PrintUrl { get; init; }
}