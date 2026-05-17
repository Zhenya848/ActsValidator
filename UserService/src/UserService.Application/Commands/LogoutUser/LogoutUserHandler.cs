using CSharpFunctionalExtensions;
using UserService.Application.Abstractions;
using UserService.Application.Repositories;
using UserService.Domain.Shared;

namespace UserService.Application.Commands.LogoutUser;

public class LogoutUserHandler : ICommandHandler<Guid, UnitResult<ErrorList>>
{
    private readonly IAuthRepository _authRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutUserHandler(IAuthRepository authRepository, IUnitOfWork unitOfWork)
    {
        _authRepository = authRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<UnitResult<ErrorList>> Handle(Guid refreshToken, CancellationToken cancellationToken = default)
    {
        var oldRefreshSession = await _authRepository
            .GetByRefreshToken(refreshToken, cancellationToken);

        if (oldRefreshSession.IsFailure)
            return (ErrorList)oldRefreshSession.Error;
        
        _authRepository.Delete(oldRefreshSession.Value, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

        return Result.Success<ErrorList>();
    }
}