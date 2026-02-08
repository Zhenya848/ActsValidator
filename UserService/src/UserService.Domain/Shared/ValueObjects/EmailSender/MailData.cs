namespace UserService.Domain.Shared.ValueObjects.EmailSender;

public record MailData(string To, string Subject, string Body);