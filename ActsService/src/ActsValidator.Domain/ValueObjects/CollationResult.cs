using ActsValidator.Domain.Shared;

namespace ActsValidator.Domain.ValueObjects;

public record CollationResult(HashSet<Discrepancy> Errors, int CoincidencesCount);