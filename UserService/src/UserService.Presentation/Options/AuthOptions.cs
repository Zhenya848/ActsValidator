namespace UserService.Presentation.Options;

public class AuthOptions
{
    public const string Auth = "AuthOptions";
    public bool CreateNewKeys { get; init; }
    public int ExpiredMinutesTime { get; init; }
    public int ExpiredDaysTimeRefreshToken { get; init; }
    public string PrivateKeyPath { get; init; }
    public string PublicKeyPath { get; init; }
    public string SecretKey { get; init; }
}