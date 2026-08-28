using ChatService.Abstractions;
using ChatService.DbContexts;
using ChatService.Models.Email;
using ChatService.Models.Event;
using ChatService.Models.Outbox;
using ChatService.Providers;
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace ChatService.Workers.Outbox;

public class ProcessOutboxMessagesService
{
    private readonly AppDbContext _dbContext;
    private readonly SupportEmailsProvider _supportEmailsProvider;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<ProcessOutboxMessagesService> _logger;
    
    private readonly ResiliencePipeline _pipeline;

    public ProcessOutboxMessagesService(
        AppDbContext dbContext,
        SupportEmailsProvider supportEmailsProvider,
        IEmailSender emailSender,
        ILogger<ProcessOutboxMessagesService> logger)
    {
        _dbContext = dbContext;
        _supportEmailsProvider = supportEmailsProvider;
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

    public async Task Execute(CancellationToken cancellationToken)
    {
        var messages = await _dbContext.OutboxMessages
            .OrderBy(o => o.OccurredOn)
            .Where(p => p.ProcessedOn == null)
            .Take(50)
            .ToListAsync(cancellationToken);
        
        if (messages.Count == 0)
            return;
        
        foreach (var message in messages)
        {
            await ProcessMessageAsync(
                message,
                cancellationToken);
        }

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
        OutboxMessage outboxMessage,
        CancellationToken cancellationToken)
    {
        var emails = _supportEmailsProvider.Emails
            .Select(e => e.Email)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        
        if (emails.Length == 0)
        {
            _logger.LogError(
                "No support emails configured.");

            return;
        }

        var type = Type.GetType(outboxMessage.Type)
                   ?? throw new Exception($"Could not find type {outboxMessage.Type}");
            
        var deserializedMessage = JsonSerializer.Deserialize(outboxMessage.Payload, type) 
            ?? throw new JsonSerializationException($"Unable to deserialize message {outboxMessage.Payload}");
            
        var @event = deserializedMessage as MessageWasSentEvent;

        if (@event is null)
        {
            _logger.LogWarning("Could not deserialize message {payload}", outboxMessage.Payload);
            return;
        }

        foreach (var email in emails)
        {
            try
            {
                await _pipeline.ExecuteAsync(
                    async token =>
                    {
                        await _emailSender.SendMessageNotificationToSupports(
                            @event.ChatId,
                            email,
                            @event.MessageContent);
                    },
                    cancellationToken);
                
                outboxMessage.ProcessedOn = DateTime.UtcNow;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send notification to support email {Email}",
                    email);
            }
        }
        
        throw new InvalidOperationException(
            $"Could not send notification for outbox {outboxMessage.Id}");
    }
}