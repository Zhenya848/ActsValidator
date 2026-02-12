using CSharpFunctionalExtensions;
using UserService.Domain;
using UserService.Domain.Shared;

namespace UserService.Application.Repositories;

public interface IAuthRepository
{
    void Delete(RefreshSession refreshSession);
    
    Task<Result<RefreshSession, Error>> GetByRefreshToken(
        Guid refreshToken,
        CancellationToken cancellationToken = default);
}