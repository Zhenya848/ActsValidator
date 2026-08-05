namespace UserService.Domain.User;

public class AdminAccount
{
    public const string ADMIN = "Admin";
    
    public Guid Id { get; private set; }
    
    public Guid UserId { get; private set; }
    public User User { get; private set; }

    private AdminAccount()
    {
        
    }
    
    public static AdminAccount CreateAdmin(User user)
    {
        return new AdminAccount()
        {
            Id = Guid.NewGuid(),
            User = user
        };
    }
}