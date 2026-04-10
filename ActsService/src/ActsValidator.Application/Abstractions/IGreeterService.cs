using ActsValidator.Domain.Shared;
using CSharpFunctionalExtensions;

namespace ActsValidator.Application.Abstractions;

public interface IGreeterService
{
    public Task<UnitResult<ErrorList>> MakeAction(
        Guid userId,
        CancellationToken cancellationToken = default);
}