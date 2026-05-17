using CSharpFunctionalExtensions;
using UserService.Domain.Shared;

namespace UserService.Domain.ValueObjects;

public record UserAccess
{
    public int TokenBalance { get; private set; } = UserConstants.TRIAL_USER_BALANSE;
    public DateTime? SubscriptionExpireAt { get; set; }
    public bool IsSubscribed => SubscriptionExpireAt is not null && SubscriptionExpireAt > DateTime.UtcNow;

    public UserAccess()
    {
        
    }
    
    public void TopUpBalance(int amount) =>
        TokenBalance += amount;

    public UnitResult<Error> DebitBalance(int amount)
    {
        if (TokenBalance < amount)
            return Errors.User.InvalidBalance();
        
        TokenBalance -= amount;
        
        return Result.Success<Error>();
    }
    
    public UnitResult<Error> Subscribe(int months)
    {
        if (months < 1)
            return Errors.General.ValueIsInvalid(nameof(months));
        
        if (SubscriptionExpireAt is not null && SubscriptionExpireAt.Value > DateTime.UtcNow)
            SubscriptionExpireAt = SubscriptionExpireAt.Value.AddMonths(months);
        else 
            SubscriptionExpireAt = DateTime.UtcNow.AddMonths(months);
        
        return Result.Success<Error>();
    }
}