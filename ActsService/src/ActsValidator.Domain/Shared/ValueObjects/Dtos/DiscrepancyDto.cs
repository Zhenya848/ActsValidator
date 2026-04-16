namespace ActsValidator.Domain.Shared.ValueObjects.Dtos;

public record DiscrepancyDto
{
    public int? Act1Row { get; init; }
    public int? Act2Row { get; init; }
    public string Act1Value { get; init; }
    public string Act2Value { get; init; }

    public string Difference { get; init; }
    public string Field { get; init; }
    public string Severity { get; init; }
    
    private DiscrepancyDto() { }

    public DiscrepancyDto(int? act1Row, int? act2Row, 
        string act1Value, string act2Value, string field, string difference, string severity)
    {
        Act1Row = act1Row;
        Act2Row = act2Row;
        Act1Value = act1Value;
        Act2Value = act2Value;
        Field = field;
        Difference = difference;
        Severity = severity;
    }
}