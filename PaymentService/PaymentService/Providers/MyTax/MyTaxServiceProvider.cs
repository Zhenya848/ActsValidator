using PaymentService.Abstractions;
using PaymentService.Providers.MyTax.Responces;

namespace PaymentService.Providers.MyTax;

public class MyTaxServiceProvider : ITaxServiceProvider
{
    private readonly IMoyNalogClient _moyNalog;
    private readonly ILogger<MyTaxServiceProvider> _logger;

    public MyTaxServiceProvider(IMoyNalogClient moyNalog, ILogger<MyTaxServiceProvider> logger)
    {
        _moyNalog = moyNalog;
        _logger = logger;
    }
    
    public async Task<SendReceiptResult> SendCheck(
        decimal amount, 
        string email, 
        string currency, 
        string productName, 
        DateTime operationTime,
        CancellationToken cancellationToken = default)
    {
        var item = new ReceiptItem()
        {
            Amount = amount,
            Name = productName,
            Quantity = 1
        };
 
        try
        {
            var result = await _moyNalog.CreateReceiptAsync([item], operationTime, cancellationToken);
 
            _logger.LogInformation(
                "Чек {ReceiptId} успешно отправлен в ФНС на сумму {Total}",
                result.ReceiptUuid,
                item.Amount * item.Quantity);
 
            return new SendReceiptResult
            {
                ReceiptId = result.ReceiptUuid,
                PrintUrl = _moyNalog.GetPrintUrl(result.ReceiptUuid)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Не удалось отправить чек в ФНС");
            throw;
        }
    }
}