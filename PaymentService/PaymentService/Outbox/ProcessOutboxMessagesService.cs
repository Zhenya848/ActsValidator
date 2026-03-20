using MassTransit;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PaymentService.DbContexts;
using Polly;
using Polly.Retry;
using JsonSerializer = System.Text.Json.JsonSerializer;
using PredicateBuilder = Polly.PredicateBuilder;

namespace PaymentService.Outbox;

public class ProcessOutboxMessagesService
{
    private readonly AppDbContext _dbContext;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ProcessOutboxMessagesService> _logger;

    public ProcessOutboxMessagesService(
        AppDbContext dbContext,
        IPublishEndpoint publishEndpoint,
        ILogger<ProcessOutboxMessagesService> logger)
    {
        _dbContext = dbContext;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task Execute(CancellationToken cancellationToken)
    {
        var messages = await _dbContext.OutboxMessages
            .OrderBy(o => o.OccurredOn)
            .Where(p => p.ProcessedOn == null)
            .Take(50)
            .ToListAsync(cancellationToken);
        
        if (messages.Count == 0)
            return;
        
        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions()
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                OnRetry = retryArgs =>
                {
                    _logger.LogCritical(retryArgs.Outcome.Exception, "Current attempt: {attempt}", retryArgs.AttemptNumber);
                    
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
        
        var tasks = messages.Select(m => ProcessMessageAsync(m, pipeline, cancellationToken));
        await Task.WhenAll(tasks);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving outbox messages");
        }
    }

    private async Task ProcessMessageAsync(
        OutboxMessage message,
        ResiliencePipeline pipeline,
        CancellationToken cancellationToken)
    {
        try
        {
            var type = Type.GetType($"{message.Type}, Core")
                ?? throw new Exception($"Could not find type {message.Type}");
            
            var deserializedMessage = JsonSerializer.Deserialize(message.Payload, type)
                ?? throw new JsonSerializationException($"Unable to deserialize message {message.Payload}");
            
            await pipeline.ExecuteAsync(async token =>
            {
                await _publishEndpoint.Publish(deserializedMessage, type, token);

                message.ProcessedOn = DateTime.UtcNow;
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            message.Error = ex.Message;
            
            _logger.LogError(ex, "Error processing message {message}", message);
        }
    }
}