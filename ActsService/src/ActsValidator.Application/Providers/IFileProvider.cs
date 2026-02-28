using ActsValidator.Domain.Shared;
using ActsValidator.Domain.Shared.ValueObjects;
using CSharpFunctionalExtensions;

namespace ActsValidator.Application.Providers;

public interface IFileProvider
{
    public Result<IEnumerable<CollationRow>, ErrorList> GetCollationRows(Stream file, bool reverse = false);
}