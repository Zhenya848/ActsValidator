using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using UserService.Application.Abstractions;

namespace UserService.Infrastructure
{
    public class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
    {
        public IDbConnection Create() =>
            new NpgsqlConnection(configuration.GetConnectionString("Database"));
    }
}
