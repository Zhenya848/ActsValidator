using ChatService.Models.Outbox;
using ChatService.Models.Shared;
using ChatService.Models.Shared.ValueObjects.Id;
using ChatService.Models.ValueObjects;
using CSharpFunctionalExtensions;
using MimeKit;

namespace ChatService.Models.Email;

public class SupportEmail : Shared.Entity<SupportEmailId>
{
    public string Email { get; }
    public SupportEmailStatus Status { get; private set; }
    public int PriorityNumber { get; }
    public DateTime CreatedAt { get; }

    private SupportEmail(SupportEmailId id) : base(id) { }

    private SupportEmail(
        SupportEmailId id,
        string email,
        SupportEmailStatus status,
        int priorityNumber,
        DateTime createdAt) : base(id)
    {
        Email = email;
        Status = status;
        PriorityNumber = priorityNumber;
        CreatedAt = createdAt;
    }
    
    public static Result<SupportEmail, Error> Create(
        string email,
        int priorityNumber)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Errors.General.ValueIsRequired("email");
        
        if (priorityNumber < 1)
            return Errors.General.ValueIsInvalid("priority number");

        if (MailboxAddress.TryParse(email, out _) == false)
            return Errors.General.ValueIsInvalid("email");

        return new SupportEmail(
            SupportEmailId.AddNewId(),
            email.Trim(),
            SupportEmailStatus.Available,
            priorityNumber,
            DateTime.UtcNow);
    }

    public void MarkAsAvailable() => Status = SupportEmailStatus.Available;
    public void MarkAsDisabled() => Status = SupportEmailStatus.Disabled;
}