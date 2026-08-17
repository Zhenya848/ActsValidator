using ChatService.Models.Outbox;
using ChatService.Models.Shared;
using ChatService.Models.Shared.ValueObjects.Id;
using ChatService.Models.ValueObjects;
using CSharpFunctionalExtensions;
using MimeKit;

namespace ChatService.Models.Email;

public class EmailDelivery : Shared.Entity<EmailDeliveryId>
{
    public Guid OutboxMessageId { get; private set; }
    public OutboxMessage OutboxMessage { get; private set; }

    public string Recipient { get; private set; }

    public EmailDeliveryStatus Status { get; private set; }

    public int Attempts { get; private set; }

    public DateTime? NextAttemptAt { get; private set; }

    public DateTime? SentAt { get; private set; }

    public string? LastError { get; private set; }

    private EmailDelivery(EmailDeliveryId id) : base(id) { }

    private EmailDelivery(
        EmailDeliveryId id,
        Guid outboxMessageId,
        string recipient) : base(id)
    {
        OutboxMessageId = outboxMessageId;
        Recipient = recipient;
        Status = EmailDeliveryStatus.Pending;
        NextAttemptAt = DateTime.UtcNow;
    }
    
    public static Result<EmailDelivery, ErrorList> Create(
        OutboxMessageId outboxMessageId,
        string recipient)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            return (ErrorList)Errors.General.ValueIsRequired("recipient");

        if (MailboxAddress.TryParse(recipient, out _) == false)
            return (ErrorList)Errors.General.ValueIsInvalid("email");

        return new EmailDelivery(
            EmailDeliveryId.AddNewId(),
            outboxMessageId,
            recipient.Trim());
    }

    public void MarkAsSending()
    {
        Status = EmailDeliveryStatus.Sending;
        Attempts++;
    }

    public void MarkAsSent()
    {
        Status = EmailDeliveryStatus.Sent;
        SentAt = DateTime.UtcNow;
        NextAttemptAt = null;
        LastError = null;
    }

    public void MarkAsFailed(
        string error,
        DateTime nextAttemptAt)
    {
        Status = EmailDeliveryStatus.Pending;
        LastError = error;
        NextAttemptAt = nextAttemptAt;
    }

    public void MarkAsDeadLetter(string error)
    {
        Status = EmailDeliveryStatus.Dead;
        LastError = error;
        NextAttemptAt = null;
    }
}