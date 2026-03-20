namespace PaymentService.Options;

public class AuthOptions
{
    public const string Auth = "AuthOptions";
    public string PublicKeyPath { get; init; }
    public string SecretKey { get; init; }
}