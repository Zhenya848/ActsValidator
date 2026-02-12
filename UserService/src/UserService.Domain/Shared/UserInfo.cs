namespace UserService.Domain.Shared;

public record UserInfo()
{
    public Guid Id { get; set; }
    
    public string UserName { get; set; }
    public string DisplayName { get; set; }
    
    public string Email { get; set; }
    
    
    public int Balance { get; set; }
    public int TrialBalance { get; set; }
}