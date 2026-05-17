using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using UserService.Domain.Shared;
using UserService.Domain.ValueObjects;

namespace UserService.Domain;

public class User : IdentityUser<Guid>
{
    public string DisplayName { get; private set; } = string.Empty;
    public UserAccess UserAccess { get; } = new();

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