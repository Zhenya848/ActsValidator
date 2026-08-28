namespace ChatService.Options;

public record SupportEmailsOptions
{
    public const string SupportEmails = "SupportEmails";
    public string[] Emails { get; init; } = [];
}