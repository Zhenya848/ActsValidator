namespace ActsValidator.Application.Collations.Queries;

public record GetCollationsWithPaginationQuery(
    Guid UserId,
    int Page,
    int PageSize,
    string? ActName,
    string? OrderBy,
    bool OrderByDesc = false);