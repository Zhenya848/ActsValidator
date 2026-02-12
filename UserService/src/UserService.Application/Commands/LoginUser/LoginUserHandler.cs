using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using UserService.Application.Abstractions;
using UserService.Domain;
using UserService.Domain.Shared;

namespace UserService.Application.Commands.LoginUser;

public class LoginUserHandler : ICommandHandler<LoginUserCommand, Result<LoginUserResponse, ErrorList>>
{
    private readonly UserManager<User> _userService;
    private readonly ITokenProvider _tokenProvider;

    public LoginUserHandler(UserManager<User> userService, ITokenProvider tokenProvider)
    {
        _userService = userService;
        _tokenProvider = tokenProvider;
    }
    
    public async Task<Result<LoginUserResponse, ErrorList>> Handle(
        LoginUserCommand command, 
        CancellationToken cancellationToken = default)
    {
        var user = await _userService.FindByEmailAsync(command.Email);

        if (user is null)
            return (ErrorList)Errors.User.NotFound(command.Email);
        
        var isPasswordCorrect = await _userService.CheckPasswordAsync(user, command.Password);
        
        if (isPasswordCorrect == false)
            return (ErrorList)Errors.User.WrongCredentials();
        
        var accessToken = _tokenProvider.GenerateAccessToken(user);
        var refreshToken = await _tokenProvider
            .GenerateRefreshToken(user, accessToken.Jti, cancellationToken);
        
        var userData = new UserInfo()
        {
            Id = user.Id,
            Email = user.Email!,
            UserName = user.Email!,
            DisplayName = user.DisplayName,
            Balance = 0,
            TrialBalance = 0
        };
        
        return new LoginUserResponse(accessToken.AccessToken, refreshToken, userData);
    }
}