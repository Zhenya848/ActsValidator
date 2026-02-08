using ActsValidator.Domain.Shared;
using ActsValidator.Domain.Shared.ValueObjects;
using CSharpFunctionalExtensions;

namespace ActsValidator.Domain.ValueObjects;

public record Discrepancy
{
    public CollationRow? Act1 { get; init; }
    public CollationRow? Act2 { get; init; }

    public string CellName { get; init; }

    private Discrepancy()
    {
        
    }
    
    private Discrepancy(CollationRow? act1, CollationRow? act2,  string cellName)
    {
        Act1 = act1;
        Act2 = act2;
        CellName = cellName;
    }

    public static Result<Discrepancy, ErrorList> Create(CollationRow? act1, CollationRow? act2, string cellName)
    {
        if (act1 is null && act2 is null)
            return (ErrorList)Error.Failure(
                "discrepancy.create.failure", 
                "Unable to create discrepancies because only 1 or 0 act can be null");
        
        if (act1 != null && act1.Equals(act2))
            return (ErrorList)Error.Failure(
            "discrepancy.create.failure", 
            "Unable to create discrepancies because act1 and act2 have common rows");

        if (string.IsNullOrEmpty(cellName))
            return (ErrorList)Errors.General.ValueIsInvalid(nameof(cellName));
        
        return new Discrepancy(act1, act2, cellName);
    }
}