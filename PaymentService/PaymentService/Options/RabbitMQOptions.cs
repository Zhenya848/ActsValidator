namespace PaymentService.Options;

public record RabbitMQOptions
{
    public const string RabbitMQ = "RabbitMQ";
    
    public string Host { get; init; }
    public string Username { get; init; }
    public string Password { get; init; }
}