using CSharpFunctionalExtensions;
using UserService.Domain;
using UserService.Domain.Shared;

namespace UserService.Application.Repositories;

public interface IAuthRepository
{
    Task<Result<Guid, Error>> Delete(
        RefreshSession refreshSession,  
        CancellationToken cancellationToken = default);
    
    Task<Result<RefreshSession, Error>> GetByRefreshToken(
        Guid refreshToken,
        CancellationToken cancellationToken = default);
}