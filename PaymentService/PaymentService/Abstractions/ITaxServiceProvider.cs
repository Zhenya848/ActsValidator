using PaymentService.Providers.MyTax.Responces;

namespace PaymentService.Abstractions;

public interface ITaxServiceProvider
{
    public Task<SendReceiptResult> SendCheck(
        decimal amount, 
        string email, 
        string currency, 
        string productName, 
        DateTime operationTime,
        CancellationToken cancellationToken = default);
}