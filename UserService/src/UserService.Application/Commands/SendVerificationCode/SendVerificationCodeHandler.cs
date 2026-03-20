using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using UserService.Application.Abstractions;
using UserService.Domain;
using UserService.Domain.Shared;

namespace UserService.Application.Commands.SendVerificationCode;

public class SendVerificationCodeHandler : ICommandHandler<Guid, UnitResult<ErrorList>>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;

    public SendVerificationCodeHandler(UserManager<User> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }
    
    public async Task<UnitResult<ErrorList>> Handle(Guid userId, CancellationToken cancellationToken = default)
    {
        var userExist = await _userManager.FindByIdAsync(userId.ToString());

        if (userExist is null)
            return (ErrorList)Errors.User.NotFound();

        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(userExist);

        var result = await _emailSender.SendVerificationCode(userId, confirmationToken, userExist.Email!);
        
        if (result.IsFailure)
            return result.Error;

        return Result.Success<ErrorList>();
    }
}