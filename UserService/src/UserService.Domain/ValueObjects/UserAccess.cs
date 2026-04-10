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

    public UnitResult<ErrorList> DebitBalance(int amount)
    {
        if (TokenBalance < amount)
            return (ErrorList)Errors.General.ValueIsInvalid(nameof(amount));
        
        TokenBalance -= amount;
        
        return Result.Success<ErrorList>();
    }
    
    public UnitResult<ErrorList> Subscribe(int months)
    {
        if (months < 1)
            return (ErrorList)Errors.General.ValueIsInvalid(nameof(months));
        
        if (SubscriptionExpireAt is not null && SubscriptionExpireAt.Value > DateTime.UtcNow)
            SubscriptionExpireAt = SubscriptionExpireAt.Value.AddMonths(months);
        else 
            SubscriptionExpireAt = DateTime.UtcNow.AddMonths(months);
        
        return Result.Success<ErrorList>();
    }

    public void Unsubscribe() => SubscriptionExpireAt = null;
}