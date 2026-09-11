using PaymentService.Outbox;
using Quartz;

namespace PaymentService.ReceiptSendingJob;

[DisallowConcurrentExecution]
public class ProcessReceiptsSendingJob : IJob
{
    private readonly ProcessReceiptsSendingService _receiptsSendingService;

    public ProcessReceiptsSendingJob(ProcessReceiptsSendingService receiptsSendingService)
    {
        _receiptsSendingService = receiptsSendingService;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        await _receiptsSendingService.Execute(context.CancellationToken);
    }
}