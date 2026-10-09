namespace ActsValidator.Domain.Shared.ValueObjects.Dtos;

public record DiscrepancyDto
{
    public CollationRow? Row1 { get; init; }
    public CollationRow? Row2 { get; init; }

    public string Difference { get; init; }
    public string Field { get; init; }
    public string Severity { get; init; }
    
    private DiscrepancyDto() { }

    public DiscrepancyDto(CollationRow? row1, CollationRow? row2, string field, string difference, string severity)
    {
        Row1 = row1; 
        Row2 = row2;
        Field = field;
        Difference = difference;
        Severity = severity;
    }
}