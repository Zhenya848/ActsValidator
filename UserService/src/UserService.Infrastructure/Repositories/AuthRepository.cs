using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Repositories;
using UserService.Domain;
using UserService.Domain.Shared;
using UserService.Infrastructure.DbContexts;

namespace UserService.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AuthDbContext _authDbContext;

    public AuthRepository(AuthDbContext authDbContext)
    {
        _authDbContext = authDbContext;
    }
    
    public void Delete(RefreshSession refreshSession)
    {
        _authDbContext.RefreshSessions.Remove(refreshSession);
    }

    public async Task<Result<RefreshSession, Error>> GetByRefreshToken(
        Guid refreshToken, 
        CancellationToken cancellationToken = default)
    {
        var refreshSession = await _authDbContext.RefreshSessions
            .Include(u => u.User)
            .FirstOrDefaultAsync(r => r.RefreshToken == refreshToken, cancellationToken);

        if (refreshSession == null)
            return Errors.General.NotFound(refreshToken);

        return refreshSession;
    }
}