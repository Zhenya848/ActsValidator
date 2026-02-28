using System.Data;
using ActsValidator.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace ActsValidator.Infrastructure
{
    public class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
    {
        public IDbConnection Create() =>
            new NpgsqlConnection(configuration.GetConnectionString("Database"));
    }
}
