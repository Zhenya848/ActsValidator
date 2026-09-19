using PaymentService.Providers.MyTax.Responces;

namespace PaymentService.Abstractions;

public interface ITaxServiceProvider
{
    public Task<SendReceiptResult> SendCheck(
        decimal amount, 
        string email, 
        string currency, 
        DateTime operationTime,
        CancellationToken cancellationToken = default);
}