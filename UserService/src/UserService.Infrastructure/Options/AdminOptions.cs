namespace UserService.Infrastructure.Options;

public class AdminOptions
{
    public const string ADMIN = "ADMIN";
    
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}