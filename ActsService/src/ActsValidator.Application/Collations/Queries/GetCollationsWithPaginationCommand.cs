namespace ActsValidator.Application.Collations.Queries;

public record GetCollationsWithPaginationCommand(
    Guid UserId,
    int Page,
    int PageSize,
    string? ActName,
    string? OrderBy);