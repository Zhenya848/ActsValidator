namespace ActsValidator.Application.Repositories;
using Collation = Domain.Collation;

public interface ICollationRepository
{
    public Guid Add(Collation collation);
}