namespace UserService.Application.Models;

public record MailOptions
{
    public const string SECTION_NAME = "MailOptions";
    
    public string From { get; init; } = string.Empty;
    public string FromDisplayName { get; init; } = string.Empty;
    public string To { get; init; } = string.Empty;
    
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 25;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public bool UseSsl { get; init; } = false;
}