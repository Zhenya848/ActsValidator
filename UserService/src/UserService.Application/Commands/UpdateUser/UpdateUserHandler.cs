using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using UserService.Application.Abstractions;
using UserService.Domain;
using UserService.Domain.Shared;

namespace UserService.Application.Commands.UpdateUser;

public class UpdateUserHandler : ICommandHandler<UpdateUserCommand, Result<UserInfo, ErrorList>>
{
    private readonly UserManager<User> _userManager;

    public UpdateUserHandler(UserManager<User> userManager)
    {
        _userManager = userManager;
    }
    
    public async Task<Result<UserInfo, ErrorList>> Handle(
        UpdateUserCommand command, 
        CancellationToken cancellationToken = default)
    {
        var userResult = await _userManager.FindByIdAsync(command.UserId.ToString());

        if (userResult is null)
            return (ErrorList)Errors.User.NotFound();

        var updateResult = userResult.Update(command.UserName, command.Email);
        
        if (updateResult.IsFailure)
            return updateResult.Error;
        
        var result = await _userManager.UpdateAsync(userResult);
            
        if (result.Succeeded == false)
            return (ErrorList)result.Errors.Select(e => Error.Failure(e.Code, e.Description)).ToList();
        
        if (string.IsNullOrWhiteSpace(command.Password) || string.IsNullOrWhiteSpace(command.NewPassword))
            return GetUserInfo(userResult);
        
        var changePasswordResult = await _userManager
            .ChangePasswordAsync(userResult, command.Password, command.NewPassword);

        if (changePasswordResult.Succeeded == false)
            return (ErrorList)changePasswordResult.Errors
                .Select(e => Error.Failure("user.wrong.credentials", e.Description))
                .ToList();
        
        return GetUserInfo(userResult);
    }
    
    private UserInfo GetUserInfo(User user) => new()
    {
        Id = user.Id,
        Email = user.Email!,
        DisplayName = user.DisplayName,
        EmailVerified = user.EmailConfirmed,
        Balance = user.UserAccess.TokenBalance,
        IsSubscribed = user.UserAccess.IsSubscribed
    };
}