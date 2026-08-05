using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using UserService.Domain.Shared;
using UserService.Domain.ValueObjects;

namespace UserService.Domain.User;

public class User : IdentityUser<Guid>
{
    private List<Role> _roles = [];
    public IReadOnlyList<Role> Roles => _roles;
    
    public string DisplayName { get; private set; } = string.Empty;
    public UserAccess UserAccess { get; } = new();
    
    public ParticipantAccount? ParticipantAccount { get; }
    public AdminAccount? AdminAccount { get; }
    
    private User()
    {
        
    }
    
    private static User Create(
        string user, 
        string email,
        Role role)
    {
        return new User
        {
            DisplayName = user,
            UserName = email,
            Email = email,
            _roles =  [role]
        };
    }
    
    public static User CreateParticipant(
        string user,
        string email,
        Role role)
    {
        if (role.Name != ParticipantAccount.PARTICIPANT)
            throw new ApplicationException($"Role {role.Name} does not exist");
        
        return Create(user, email, role);
    }
    
    public static User CreateAdmin(
        string user,
        string email,
        Role role)
    {
        if (role.Name != AdminAccount.ADMIN)
            throw new ApplicationException($"Role {role.Name} does not exist");
        
        return Create(user, email, role);
    }

    public UnitResult<ErrorList> Update(string userName, string email)
    {
        var errors = new List<Error>();
        email = email.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(userName))
            errors.Add(Errors.General.ValueIsRequired(nameof(userName)));
        
        if (email.IsEmailValid() == false)
            errors.Add(Errors.General.ValueIsInvalid(nameof(email)));
        
        if (errors.Count > 0)
            return (ErrorList)errors;
        
        if (Email != email)
            EmailConfirmed = false;

        DisplayName = userName;
        UserName = email;
        Email = email;

        return Result.Success<ErrorList>();
    }
}