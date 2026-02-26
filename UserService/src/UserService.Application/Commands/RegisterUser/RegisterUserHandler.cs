using System.Data;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Abstractions;
using UserService.Application.Models;
using UserService.Domain;
using UserService.Domain.Shared;

namespace UserService.Application.Commands.RegisterUser;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, Result<Guid, ErrorList>>
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserHandler> _logger;
    
    public RegisterUserHandler(
        UserManager<User> userManager, 
        IEmailSender emailSender, 
        IUnitOfWork unitOfWork, 
        ILogger<RegisterUserHandler> logger)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }
    
    public async Task<Result<Guid, ErrorList>> Handle(
        RegisterUserCommand command, 
        CancellationToken cancellationToken = default)
    {
        var errors = new List<Error>();
        
        if (string.IsNullOrWhiteSpace(command.UserName))
            errors.Add(Errors.General.ValueIsRequired(nameof(command.UserName)));
        
        if (string.IsNullOrWhiteSpace(command.Email))
            errors.Add(Errors.General.ValueIsRequired(nameof(command.Email)));
        
        if (string.IsNullOrWhiteSpace(command.Password))
            errors.Add(Errors.General.ValueIsRequired(nameof(command.Password)));

        if (errors.Count > 0)
            return (ErrorList)errors;

        var user = new User
        {
            Email = command.Email,
            UserName = command.Email,
            DisplayName = command.UserName
        };

        var userExist = await _userManager.FindByEmailAsync(command.Email);

        if (userExist is not null)
        {
            if (userExist.EmailConfirmed == false)
                await _userManager.DeleteAsync(userExist);
            else
                return (ErrorList)Errors.User.AlreadyExist();
        }
        
        using var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        try
        {
            var result = await _userManager.CreateAsync(user, command.Password);

            if (result.Succeeded == false)
            {
                transaction.Rollback();
                return (ErrorList)result.Errors.Select(e => Error.Failure(e.Code, e.Description)).ToList();
            }

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var confirmationLink = $"http://localhost:5172/api/auth/email-verification" +
                                   $"?userId={user.Id}&token={Base64UrlEncoder.Encode(confirmationToken)}";

            var subject = "Подтверждение регистрации";
            var body = $"Для подтверждения регистрации перейдите по ссылке: {confirmationLink}";

            var mailData = new MailData(command.Email, subject, body);

            var sendMessageResult = await _emailSender.Send(mailData);

            if (sendMessageResult.IsFailure)
            {
                transaction.Rollback();
                return sendMessageResult.Error;
            }
            
            transaction.Commit();

            return user.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            transaction.Rollback();
            
            return (ErrorList)Error.Failure("user.register.failure", ex.Message);
        }
    }
}