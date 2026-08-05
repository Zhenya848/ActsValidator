using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using UserService.Application.Repositories;
using UserService.Domain;
using UserService.Domain.Shared;
using UserService.Domain.User;
using UserService.Infrastructure.DbContexts;

namespace UserService.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AuthDbContext _authDbContext;

    public AuthRepository(AuthDbContext authDbContext)
    {
        _authDbContext = authDbContext;
    }

    public Guid CreateParticipant(ParticipantAccount participantAccount)
    {
        var addResult = _authDbContext.ParticipantAccounts.Add(participantAccount);
        
        return addResult.Entity.Id;
    }

    public Guid DeleteParticipant(Guid userId)
    {
        _authDbContext.ParticipantAccounts
            .Where(u => u.UserId == userId)
            .ExecuteDelete();
        
        return userId;
    }

    public Result<Guid, Error> Delete(
        RefreshSession refreshSession, 
        CancellationToken cancellationToken = default)
    {
        var rowsAffected = _authDbContext.RefreshSessions
            .Where(t => t.RefreshToken == refreshSession.RefreshToken)
            .ExecuteDelete();

        if (rowsAffected == 0)
            return Errors.Token.InvalidToken();

        return refreshSession.Id;
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