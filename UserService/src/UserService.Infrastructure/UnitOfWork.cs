using System.Data;
using Microsoft.EntityFrameworkCore.Storage;
using UserService.Application.Abstractions;
using UserService.Infrastructure.DbContexts;

namespace UserService.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AuthDbContext _authDbContext;

        public UnitOfWork(AuthDbContext authDbContext)
        {
            _authDbContext = authDbContext;
        }

        public async Task<IDbTransaction> BeginTransaction(CancellationToken cancellationToken = default)
        {
            var transaction = await _authDbContext.Database.BeginTransactionAsync(cancellationToken);

            return transaction.GetDbTransaction();
        }

        public async Task SaveChanges(CancellationToken cancellationToken = default)
        {
            await _authDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
