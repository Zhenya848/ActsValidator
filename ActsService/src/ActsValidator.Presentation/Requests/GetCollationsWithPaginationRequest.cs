namespace ActsValidator.Presentation.Requests;

public record GetCollationsWithPaginationRequest(
    int Page,
    int PageSize,
    string? ActName,
    string? OrderBy,
    bool OrderByDesc = false);