using ActsValidator.Domain;
using ActsValidator.Domain.Shared;
using CSharpFunctionalExtensions;

namespace ActsValidator.Application.Repositories;
using Collation = Domain.Collation;

public interface IAppRepository
{
    public Guid AddCollation(Collation collation);
    public Guid AddAiRequest(AiRequest aiRequest);
}