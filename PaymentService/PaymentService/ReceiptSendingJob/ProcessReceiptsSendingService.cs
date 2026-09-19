using Microsoft.EntityFrameworkCore;
using PaymentService.Abstractions;
using PaymentService.DbContexts;
using PaymentService.Models;
using PaymentService.Models.ValueObjects;
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
    private readonly IEmailSender _emailSender;

    public ProcessReceiptsSendingService(
        AppDbContext dbContext, 
        ILogger<ProcessReceiptsSendingService> logger,
        ITaxServiceProvider taxProvider,
        IEmailSender emailSender)
    {
        _dbContext = dbContext;
        _logger = logger;
        _taxProvider = taxProvider;
        _emailSender = emailSender;
    }
    
    public async Task Execute(CancellationToken cancellationToken)
    {
        var receipts = await _dbContext.Receipts
            .Where(p => p.ProcessedOn == null)
            .OrderBy(o => o.OccurredOn)
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

        foreach (var receipt in receipts.Where(r => r.PrintUrl is null))
            await ProcessReceipts(receipt, pipeline, cancellationToken);

        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (Exception e) { _logger.LogError(e, "Error processing message {message}", e.Message); }

        foreach (var receipt in receipts)
            await SendReceiptToEmail(receipt, pipeline, cancellationToken);
        
        try { await _dbContext.SaveChangesAsync(cancellationToken); }
        catch (Exception e) { _logger.LogError(e, "Error processing message {message}", e.Message); }
    }

    private async Task SendReceiptToEmail(
        Receipt receipt, 
        ResiliencePipeline pipeline, 
        CancellationToken cancellationToken)
    {
        try
        {
            await pipeline.ExecuteAsync(async token =>
                await _emailSender.SendReceiptToEmail(receipt, token),
                cancellationToken);
            
            receipt.ProcessedOn = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            receipt.Error = ex.Message;
            
            _logger.LogError(ex, "Error processing message {message}", ex.Message);
        }
    }
    
    private async Task ProcessReceipts(
        Receipt receipt, 
        ResiliencePipeline pipeline, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await pipeline.ExecuteAsync(async token =>
                await _taxProvider.SendCheck(
                    receipt.Amount, 
                    receipt.CustomerEmail, 
                    receipt.Currency,
                    receipt.OccurredOn,
                    token), 
                cancellationToken);

            receipt.PrintUrl = response.PrintUrl;
        }
        catch (Exception ex)
        {
            receipt.Error = ex.Message;
            
            _logger.LogError(ex, "Error processing message {message}", ex.Message);
        }
    }
}