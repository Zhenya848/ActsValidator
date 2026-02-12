using CSharpFunctionalExtensions;
using UserService.Application.Abstractions;
using UserService.Application.Repositories;
using UserService.Domain.Shared;

namespace UserService.Application.Commands.RefreshToken;

public class RefreshTokenHandler : ICommandHandler<RefreshTokenCommand, Result<LoginUserResponse, ErrorList>>
{
    private readonly IAuthRepository _authRepository;
    private readonly ITokenProvider _tokenProvider;
    private readonly IUnitOfWork _unitOfWork;
    
    public RefreshTokenHandler(IAuthRepository authRepository, ITokenProvider tokenProvider, IUnitOfWork unitOfWork)
    {
        _authRepository = authRepository;
        _tokenProvider = tokenProvider;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Result<LoginUserResponse, ErrorList>> Handle(
        RefreshTokenCommand command, 
        CancellationToken cancellationToken = default)
    {
        var oldRefreshSession = await _authRepository
            .GetByRefreshToken(command.RefreshToken, cancellationToken);

        if (oldRefreshSession.IsFailure)
            return (ErrorList)oldRefreshSession.Error;
        
        var userClaims = await _tokenProvider.GetUserClaims(command.AccessToken);
        
        if (userClaims.IsFailure)
            return userClaims.Error;
        
        var userIdStr = userClaims.Value.FirstOrDefault(s => s.Type == CustomClaims.Sub)?.Value;

        if (Guid.TryParse(userIdStr, out var userId) == false)
            return (ErrorList)Errors.General.Failure(userIdStr);
        
        if (oldRefreshSession.Value.UserId != userId)
            return (ErrorList)Errors.Token.InvalidToken();
        
        var userJtiStr = userClaims.Value.FirstOrDefault(s => s.Type == CustomClaims.Jti)?.Value;
        
        if (Guid.TryParse(userJtiStr, out var userJti) == false)
            return (ErrorList)Errors.General.Failure(userJtiStr);
        
        if (userJti != oldRefreshSession.Value.Jti)
            return (ErrorList)Errors.Token.InvalidToken();
        
        _authRepository.Delete(oldRefreshSession.Value);
        await _unitOfWork.SaveChanges(cancellationToken);
        
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