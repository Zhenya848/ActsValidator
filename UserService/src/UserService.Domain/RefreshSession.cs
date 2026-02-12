namespace UserService.Domain;

public class RefreshSession
{
    public User User { get; init; }
    
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid RefreshToken { get; init; }
    public Guid Jti { get; init; }
    
    public DateTime CreatedAt { get; init; }
    public DateTime ExpiresIn { get; init; }
}