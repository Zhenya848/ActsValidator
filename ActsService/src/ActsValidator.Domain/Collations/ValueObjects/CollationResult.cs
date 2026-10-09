namespace ActsValidator.Domain.Collations.ValueObjects;

public record CollationResult(HashSet<Discrepancy> Errors, int CoincidencesCount);