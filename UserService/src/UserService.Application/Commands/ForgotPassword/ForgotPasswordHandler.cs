using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using UserService.Application.Abstractions;
using UserService.Domain;
using UserService.Domain.Shared;

namespace UserService.Application.Commands.ForgotPassword;

public class ForgotPasswordHandler : ICommandHandler<string, UnitResult<ErrorList>>
{
    private readonly UserManager<User> _userManager;
    private IEmailSender _emailSender;

    public ForgotPasswordHandler(UserManager<User> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }
    
    public async Task<UnitResult<ErrorList>> Handle(
        string email, 
        CancellationToken cancellationToken = default)
    {
        var userExist = await _userManager.FindByEmailAsync(email);

        if (userExist is null)
            return (ErrorList)Errors.User.NotFound();
        
        var token = await _userManager.GeneratePasswordResetTokenAsync(userExist);
        var result = await _emailSender.SendPasswordResetCode(userExist.Id, token, userExist.Email!);
        
        if (result.IsFailure)
            return result.Error;

        return Result.Success<ErrorList>();
    }
}