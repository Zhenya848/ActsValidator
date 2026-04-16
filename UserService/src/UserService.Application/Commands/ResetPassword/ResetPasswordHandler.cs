using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Abstractions;
using UserService.Domain;
using UserService.Domain.Shared;

namespace UserService.Application.Commands.ResetPassword;

public class ResetPasswordHandler : ICommandHandler<ResetPasswordCommand, UnitResult<ErrorList>>
{
    private readonly UserManager<User> _userManager;

    public ResetPasswordHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<UnitResult<ErrorList>> Handle(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var userExist = await _userManager.FindByIdAsync(command.UserId.ToString());
        
        if (userExist is null)
            return (ErrorList)Errors.User.NotFound();
        
        var result = await _userManager
            .ResetPasswordAsync(userExist, Base64UrlEncoder.Decode(command.Token), command.NewPassword);
        
        if (result.Succeeded == false)
            return (ErrorList)result.Errors.Select(x => Error.Failure(x.Code, x.Description)).ToList();

        return Result.Success<ErrorList>();
    }
}