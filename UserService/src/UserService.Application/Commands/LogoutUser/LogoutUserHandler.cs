using CSharpFunctionalExtensions;
using UserService.Application.Abstractions;
using UserService.Application.Repositories;
using UserService.Domain.Shared;

namespace UserService.Application.Commands.LogoutUser;

public class LogoutUserHandler : ICommandHandler<Guid, UnitResult<ErrorList>>
{
    private readonly IAuthRepository _authRepository;

    public LogoutUserHandler(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }
    
    public async Task<UnitResult<ErrorList>> Handle(Guid refreshToken, CancellationToken cancellationToken = default)
    {
        var oldRefreshSession = await _authRepository
            .GetByRefreshToken(refreshToken, cancellationToken);

        if (oldRefreshSession.IsFailure)
            return (ErrorList)oldRefreshSession.Error;
        
        await _authRepository.Delete(oldRefreshSession.Value, cancellationToken);

        return Result.Success<ErrorList>();
    }
}