namespace PaymentService.EmailSender;

public record MailData(string To, string Subject, string Body);