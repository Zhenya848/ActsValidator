using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Abstractions;
using UserService.Domain;
using UserService.Domain.Shared;

namespace UserService.Application.Commands.VerifyEmail;

public class VerifyEmailHandler : ICommandHandler<VerifyEmailCommand, Result<Guid, ErrorList>>
{
    private readonly UserManager<User> _userManager;

    public VerifyEmailHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<Result<Guid, ErrorList>> Handle(
        VerifyEmailCommand command, 
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
            return (ErrorList)Errors.User.NotFound();
        
        var result = await _userManager.ConfirmEmailAsync(user, Base64UrlEncoder.Decode(command.Token));

        if (result.Succeeded == false)
            return (ErrorList)result.Errors.Select(e => Error.Failure(e.Code, e.Description)).ToList();
        
        return user.Id;
    }
}