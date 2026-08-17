using ChatService.DbContexts;
using ChatService.Models.Email;
using ChatService.Models.Event;
using ChatService.Models.Outbox;
using ChatService.Providers;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Workers.Outbox;

public class ProcessOutboxMessagesService
{
    private readonly AppDbContext _dbContext;
    private readonly SupportEmailsProvider _supportEmailsProvider;
    private readonly ILogger<ProcessOutboxMessagesService> _logger;

    public ProcessOutboxMessagesService(
        AppDbContext dbContext,
        SupportEmailsProvider supportEmailsProvider,
        ILogger<ProcessOutboxMessagesService> logger)
    {
        _dbContext = dbContext;
        _supportEmailsProvider = supportEmailsProvider;
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
        var emails = (await _supportEmailsProvider
                .GetSupportEmails(cancellationToken))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        
        if (emails.Length == 0)
        {
            throw new InvalidOperationException(
                "No support recipients configured.");
        }

        var deliveries = emails
            .Select(email => EmailDelivery.Create(
                outboxMessage.Id,
                email))
            .ToList();

        if (deliveries.Any(d => d.IsFailure))
        {
            _logger.LogError("Failed to send outbox messages.");
            return;
        }

        _dbContext.EmailDeliveries.AddRange(deliveries.Select(d => d.Value));

        outboxMessage.ProcessedOn = DateTime.UtcNow;
    }
}