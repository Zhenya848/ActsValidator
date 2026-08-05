
namespace UserService.Domain.User;

public class ParticipantAccount
{
    public const string PARTICIPANT = "Participant";
    
    public Guid Id { get; private set; }
    
    public Guid UserId { get; private set; }
    public User User { get; private set; }

    private ParticipantAccount()
    {
        
    }
    
    public static ParticipantAccount CreateParticipant(User user)
    {
        return new ParticipantAccount()
        {
            Id = Guid.NewGuid(),
            User = user
        };
    }
}