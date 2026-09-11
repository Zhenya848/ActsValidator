namespace ChatService.EmailSender;

public record SupportEmailsOptions
{
    public const string SupportEmails = "SupportEmails";
    public string[] Emails { get; init; } = [];
}