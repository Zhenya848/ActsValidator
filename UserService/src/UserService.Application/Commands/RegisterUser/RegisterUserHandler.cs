using System.Data;
using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using UserService.Application.Abstractions;
using UserService.Application.Models;
using UserService.Application.Repositories;
using UserService.Domain;
using UserService.Domain.Shared;
using UserService.Domain.User;

namespace UserService.Application.Commands.RegisterUser;

public class RegisterUserHandler : ICommandHandler<RegisterUserCommand, Result<Guid, ErrorList>>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IEmailSender _emailSender;
    private readonly IAuthRepository _authRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RegisterUserHandler> _logger;
    
    public RegisterUserHandler(
        UserManager<User> userManager, 
        RoleManager<Role> roleManager,
        IEmailSender emailSender, 
        IAuthRepository authRepository,
        IUnitOfWork unitOfWork, 
        ILogger<RegisterUserHandler> logger)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
        _authRepository = authRepository;
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
        
        if (command.Email.IsEmailValid() == false)
            errors.Add(Errors.General.ValueIsInvalid(nameof(command.Email)));
        
        if (string.IsNullOrWhiteSpace(command.Password))
            errors.Add(Errors.General.ValueIsRequired(nameof(command.Password)));

        if (errors.Count > 0)
            return (ErrorList)errors;
        
        var role = await _roleManager.FindByNameAsync(ParticipantAccount.PARTICIPANT)
                   ?? throw new ApplicationException($"Role {ParticipantAccount.PARTICIPANT} does not exist");
        
        var user = User.CreateParticipant(command.UserName, command.Email, role);
        var participantAccount = ParticipantAccount.CreateParticipant(user);

        var userExist = await _userManager.FindByEmailAsync(command.Email);

        if (userExist is not null)
        {
            if (userExist.EmailConfirmed == false)
            {
                await _userManager.DeleteAsync(userExist);
                _authRepository.DeleteParticipant(userExist.Id);
            }
            else
                return (ErrorList)Errors.User.AlreadyExist();
        }
        
        using var transaction = await _unitOfWork.BeginTransaction(cancellationToken);

        try
        {
            _authRepository.CreateParticipant(participantAccount);
            var result = await _userManager.CreateAsync(user, command.Password);
            
            if (result.Succeeded == false)
            {
                transaction.Rollback();
                return (ErrorList)result.Errors.Select(e => Error.Failure(e.Code, e.Description)).ToList();
            }

            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            var sendVerificationCodeResult = await _emailSender
                .SendVerificationCode(user.Id, confirmationToken, command.Email);

            if (sendVerificationCodeResult.IsFailure)
            {
                transaction.Rollback();
                return sendVerificationCodeResult.Error;
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