using CSharpFunctionalExtensions;
using Dapper;
using Microsoft.Extensions.Logging;
using UserService.Application.Abstractions;
using UserService.Domain.Shared;

namespace UserService.Application.Queries.GetUser;

public class GetUserHandler : IQueryHandler<Guid, Result<UserInfo, ErrorList>>
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<GetUserHandler> _logger;

    public GetUserHandler(ISqlConnectionFactory connectionFactory, ILogger<GetUserHandler> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }
    
    public async Task<Result<UserInfo, ErrorList>> Handle(Guid refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.Create();

            var sql = $@"
                SELECT 
                    u.id AS Id,
                    u.user_name AS UserName,
                    u.display_name AS DisplayName,
                    u.email AS Email,
                    u.email_confirmed AS EmailVerified
                FROM refresh_sessions rs
                JOIN users u ON rs.user_id = u.id
                WHERE rs.refresh_token = @RefreshToken
                LIMIT 1";

            var users = await connection.QueryAsync<UserInfo>(
                sql,
                new { RefreshToken = refreshToken }
            );

            var result = users as UserInfo[] ?? users.ToArray();

            if (result.Length < 1)
                return (ErrorList)Errors.User.NotFound();

            return result[0];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return (ErrorList)Error.Failure("get.user.failure", ex.Message);
        }
    }
}