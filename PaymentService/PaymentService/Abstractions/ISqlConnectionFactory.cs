using System.Data;

namespace PaymentService.Abstractions
{
    public interface ISqlConnectionFactory
    {
        public IDbConnection Create();
    }
}
