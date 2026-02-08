using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Abstractions;
using UserService.Application.EmailSender;
using UserService.Domain;
using UserService.Domain.Shared;
using UserService.Domain.Shared.ValueObjects.EmailSender;

namespace UserService.Application.Commands.RegisterUser;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, Result<Guid, ErrorList>>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;

    public RegisterUserHandler(UserManager<User> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }
    
    public async Task<Result<Guid, ErrorList>> Handle(
        RegisterUserCommand command, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.UserName))
            return (ErrorList)Errors.General.ValueIsRequired(nameof(command.UserName));
        
        if (string.IsNullOrWhiteSpace(command.Email))
            return (ErrorList)Errors.General.ValueIsRequired(nameof(command.Email));
        
        if (string.IsNullOrWhiteSpace(command.Password))
            return (ErrorList)Errors.General.ValueIsRequired(nameof(command.Password));

        var user = new User
        {
            Email = command.Email,
            UserName = command.UserName
        };
        
        var result = await _userManager.CreateAsync(user, command.Password);

        if (result.Succeeded == false)
            return (ErrorList)result.Errors.Select(e => Error.Failure(e.Code, e.Description)).ToList();
        
        var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        
        var confirmationLink = $"http://localhost:5172/api/auth/email-verification" +
                               $"?userId={user.Id}&token={Base64UrlEncoder.Encode(confirmationToken)}";

        var subject = "Подтверждение регистрации";
        var body = $"Для подтверждения регистрации перейдите по ссылке: {confirmationLink}";
        
        var mailData = new MailData(command.Email, subject, body);

        await _emailSender.Send(mailData);

        return user.Id;
    }
}