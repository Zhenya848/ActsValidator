using System.Data;

namespace ActsValidator.Application.Abstractions
{
    public interface ISqlConnectionFactory
    {
        public IDbConnection Create();
    }
}
