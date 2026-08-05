using System.Data;

namespace ChatService.Abstractions
{
    public interface ISqlConnectionFactory
    {
        public IDbConnection Create();
    }
}
