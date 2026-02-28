using CSharpFunctionalExtensions;
using Dapper;
using UserService.Application.Abstractions;
using UserService.Domain.Shared;

namespace UserService.Application.Queries.GetUser;

public class GetUserHandler : IQueryHandler<Guid, Result<UserInfo, ErrorList>>
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public GetUserHandler(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }
    
    public async Task<Result<UserInfo, ErrorList>> Handle(Guid userId, CancellationToken cancellationToken = default)
    {
        using var connection = _connectionFactory.Create();

        var sql = @"
                SELECT 
                    id as Id, 
                    user_name as UserName, 
                    display_name as DisplayName, 
                    email as Email, 
                    balance as Balance, 
                    trial_balance as TrialBalance
                FROM users 
                WHERE id = @Id";
            
        var user = await connection.QuerySingleOrDefaultAsync<UserInfo>(sql, new { Id = userId });

        if (user is null)
            return (ErrorList)Errors.User.NotFound();
            
        return user;
    }
}