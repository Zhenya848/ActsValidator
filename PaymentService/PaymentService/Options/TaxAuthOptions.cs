namespace PaymentService.Options;

public class TaxAuthOptions
{
    public const string TaxAuth = "TaxAuthOptions";
    
    public string Inn { get; set; }
    public string Password { get; set; }
    public string PhoneNumber { get; set; }
    public string SourceDeviceId { get; set; }
    
    public int RefreshTokenExpiredInDays { get; set; }
}