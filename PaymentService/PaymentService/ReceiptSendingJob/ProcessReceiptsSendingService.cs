using Microsoft.EntityFrameworkCore;
using PaymentService.Abstractions;
using PaymentService.DbContexts;
using PaymentService.Models;
using PaymentService.Providers.MyTax;
using Polly;
using Polly.Retry;
using PredicateBuilder = System.Linq.PredicateBuilder;

namespace PaymentService.ReceiptSendingJob;

public class ProcessReceiptsSendingService
{
    private readonly AppDbContext _dbContext;
    private readonly ITaxServiceProvider _taxProvider;
    private readonly ILogger<ProcessReceiptsSendingService> _logger;

    public ProcessReceiptsSendingService(
        AppDbContext dbContext, 
        ILogger<ProcessReceiptsSendingService> logger,
        ITaxServiceProvider taxProvider)
    {
        _dbContext = dbContext;
        _logger = logger;
        _taxProvider = taxProvider;
    }
    
    public async Task Execute(CancellationToken cancellationToken)
    {
        var receipts = await _dbContext.Receipts
            .OrderBy(o => o.OccurredOn)
            .Where(p => p.ProcessedOn == null)
            .Take(10)
            .ToListAsync(cancellationToken);
        
        if (receipts.Count == 0)
            return;
        
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions()
            {
                MaxRetryAttempts = 2,
                BackoffType = DelayBackoffType.Constant,
                Delay = TimeSpan.FromSeconds(3),
                ShouldHandle = new Polly.PredicateBuilder().Handle<Exception>(),
                OnRetry = retryArgs =>
                {
                    _logger.LogWarning(
                        retryArgs.Outcome.Exception, 
                        "Current attempt: {attempt}", retryArgs.AttemptNumber);
                    
                    return ValueTask.CompletedTask;
                }
            })
            .Build();

        foreach (var receipt in receipts)
            await ProcessReceipts(receipt, pipeline, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error processing message {message}", e.Message);
        }
    }

    private async Task ProcessReceipts(
        Receipt receipt, 
        ResiliencePipeline pipeline, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            await pipeline.ExecuteAsync(async token =>
                await _taxProvider.SendCheck(
                    receipt.Amount, 
                    receipt.CustomerEmail, 
                    receipt.Currency,
                    receipt.ProductName,
                    receipt.OccurredOn,
                    token), 
                cancellationToken);
            
            receipt.ProcessedOn = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            receipt.Error = ex.Message;
            
            _logger.LogError(ex, "Error processing message {message}", ex.Message);
        }
    }
}