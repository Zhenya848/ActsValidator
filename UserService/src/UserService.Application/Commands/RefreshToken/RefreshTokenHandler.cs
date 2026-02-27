using CSharpFunctionalExtensions;
using UserService.Application.Abstractions;
using UserService.Application.Repositories;
using UserService.Domain.Shared;

namespace UserService.Application.Commands.RefreshToken;

public class RefreshTokenHandler : ICommandHandler<Guid, Result<LoginUserResponse, ErrorList>>
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenProvider _tokenProvider;
    
    public RefreshTokenHandler(IAuthRepository authRepository, ITokenProvider tokenProvider)
    {
        _authRepository = authRepository;
        _tokenProvider = tokenProvider;
    }
    
    public async Task<Result<LoginUserResponse, ErrorList>> Handle(
        Guid refreshToken, 
        CancellationToken cancellationToken = default)
    {
        var oldRefreshSession = await _authRepository
            .GetByRefreshToken(refreshToken, cancellationToken);

        if (oldRefreshSession.IsFailure)
            return (ErrorList)oldRefreshSession.Error;
        
        if (oldRefreshSession.Value.ExpiresIn < DateTime.UtcNow)
            return (ErrorList)Errors.Token.InvalidToken();
        
        var deleteResult = await _authRepository.Delete(oldRefreshSession.Value, cancellationToken);

        if (deleteResult.IsFailure)
            return (ErrorList)deleteResult.Error;
        
        var accessToken = _tokenProvider
            .GenerateAccessToken(oldRefreshSession.Value.User);
        
        var newRefreshToken = await _tokenProvider
            .GenerateRefreshToken(oldRefreshSession.Value.User, accessToken.Jti, cancellationToken);
        
        var userData = new UserInfo()
        {
            Id = oldRefreshSession.Value.User.Id,
            Email = oldRefreshSession.Value.User.Email!,
            UserName = oldRefreshSession.Value.User.Email!,
            DisplayName = oldRefreshSession.Value.User.DisplayName,
            Balance = 0,
            TrialBalance = 0
        };

        return new LoginUserResponse(accessToken.AccessToken, newRefreshToken, userData);
    }
}