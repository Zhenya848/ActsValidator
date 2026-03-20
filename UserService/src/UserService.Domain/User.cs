using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using UserService.Domain.Shared;

namespace UserService.Domain;

public class User : IdentityUser<Guid>
{
    public string DisplayName { get; private set; }

    public UnitResult<ErrorList> Update(string userName, string email)
    {
        var errors = new List<Error>();

        if (string.IsNullOrWhiteSpace(userName))
            errors.Add(Errors.General.ValueIsInvalid(nameof(userName)));
        
        if (string.IsNullOrWhiteSpace(email))
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