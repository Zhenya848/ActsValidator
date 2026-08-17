using ChatService.Abstractions;
using ChatService.DbContexts;
using ChatService.EmailSender;
using ChatService.Models.Email;
using ChatService.Models.Event;
using ChatService.Models.ValueObjects;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ChatService.Workers.Email;

public sealed class ProcessEmailDeliveriesService
{
    private const int BatchSize = 50;
    private const int MaxAttempts = 5;

    private readonly AppDbContext _dbContext;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ProcessEmailDeliveriesService> _logger;

    private readonly ResiliencePipeline _pipeline;

    public ProcessEmailDeliveriesService(
        AppDbContext dbContext,
        IEmailSender emailSender,
        ILogger<ProcessEmailDeliveriesService> logger)
    {
        _dbContext = dbContext;
        _emailSender = emailSender;
        _logger = logger;

        _pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 2,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromSeconds(1),
                ShouldHandle = new PredicateBuilder()
                    .Handle<SmtpCommandException>()
                    .Handle<SmtpProtocolException>()
                    .Handle<IOException>()
            })
            .Build();
    }

    public async Task Execute(
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var deliveries = await _dbContext.EmailDeliveries
            .Where(x =>
                x.Status == EmailDeliveryStatus.Pending &&
                x.NextAttemptAt <= now)
            .OrderBy(x => x.NextAttemptAt)
            .Take(BatchSize)
            .Include(o => o.OutboxMessage)
            .ToListAsync(cancellationToken);

        var groups = deliveries.GroupBy(x => x.OutboxMessageId);

        foreach (var group in groups)
        {
            var message = group.First().OutboxMessage;
            
            var type = Type.GetType(message.Type)
                ?? throw new Exception($"Could not find type {message.Type}");
            
            var deserializedMessage = JsonSerializer.Deserialize(message.Payload, type) 
                ?? throw new JsonSerializationException($"Unable to deserialize message {message.Payload}");
            
            var @event = deserializedMessage as MessageWasSentEvent;

            if (@event is null)
            {
                _logger.LogWarning("Could not deserialize message {payload}", message.Payload);
                continue;
            }

            foreach (var delivery in group)
                await ProcessDeliveryAsync(delivery, @event, cancellationToken);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ProcessDeliveryAsync(
        EmailDelivery delivery,
        MessageWasSentEvent @event,
        CancellationToken cancellationToken)
    {
        try
        {
            await _pipeline.ExecuteAsync(
                async token =>
                {
                    delivery.MarkAsSending();
                    
                    await _emailSender.SendMessageNotificationToSupports(
                        @event.ChatId, delivery.Recipient, @event.MessageContent);
                },
                cancellationToken);

            delivery.MarkAsSent();

            _logger.LogInformation(
                "Email {DeliveryId} successfully sent to {Recipient}",
                delivery.Id,
                delivery.Recipient);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send email {DeliveryId} to {Recipient}",
                delivery.Id,
                delivery.Recipient);

            if (delivery.Attempts >= MaxAttempts)
            {
                delivery.MarkAsDeadLetter(ex.Message);

                _logger.LogError(
                    "Email delivery {DeliveryId} moved to dead letter",
                    delivery.Id);

                return;
            }

            var nextAttemptAt = CalculateNextAttempt(
                delivery.Attempts);

            delivery.MarkAsFailed(
                ex.Message,
                nextAttemptAt);
        }
    }

    private static DateTime CalculateNextAttempt(int attempts)
    {
        return DateTime.UtcNow.Add(
            attempts switch
            {
                1 => TimeSpan.FromSeconds(30),
                2 => TimeSpan.FromMinutes(2),
                3 => TimeSpan.FromMinutes(10),
                4 => TimeSpan.FromMinutes(30),
                _ => TimeSpan.FromHours(2)
            });
    }
}