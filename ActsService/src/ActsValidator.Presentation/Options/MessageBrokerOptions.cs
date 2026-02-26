namespace ActsValidator.Presentation.Options;

public record MessageBrokerOptions
{
    public const string MessageBroker = "MessageBroker";
    
    public string Host { get; init; }
    public string Username { get; init; }
    public string Password { get; init; }
}