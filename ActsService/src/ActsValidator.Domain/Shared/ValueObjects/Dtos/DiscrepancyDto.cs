namespace ActsValidator.Domain.Shared.ValueObjects.Dtos;

public record DiscrepancyDto
{
    public CollationRowDto? Act1 { get; init; }
    public CollationRowDto? Act2 { get; init; }

    public string CellName { get; init; }
    
    private DiscrepancyDto() { }

    public DiscrepancyDto(CollationRowDto? act1, CollationRowDto? act2, string cellName)
    {
        Act1 = act1;
        Act2 = act2;
        CellName = cellName;
    }
}