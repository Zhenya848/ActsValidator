using CSharpFunctionalExtensions;
using UserService.Domain;
using UserService.Domain.Shared;
using UserService.Domain.User;

namespace UserService.Application.Repositories;

public interface IAuthRepository
{
    public Guid CreateParticipant(ParticipantAccount participantAccount);
    
    public Guid DeleteParticipant(Guid userId);
    
    Result<Guid, Error> Delete(
        RefreshSession refreshSession,  
        CancellationToken cancellationToken = default);
    
    Task<Result<RefreshSession, Error>> GetByRefreshToken(
        Guid refreshToken,
        CancellationToken cancellationToken = default);
}