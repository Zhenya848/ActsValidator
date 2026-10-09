using ActsValidator.Domain;
using ActsValidator.Domain.Collations;
using ActsValidator.Domain.Shared;
using CSharpFunctionalExtensions;

namespace ActsValidator.Application.Repositories;
using Collation = Collation;

public interface IAppRepository
{
    public Guid AddCollation(Collation collation);
}