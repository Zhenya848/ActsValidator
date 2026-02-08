using CSharpFunctionalExtensions;

namespace ActsValidator.Domain.Shared.ValueObjects;

public record CollationRow : IEquatable<CollationRow>
{
    public int SerialNumber { get; init; } = 1;
    public DateTime Date { get; init; } = DateTime.Now;
    public decimal Debet { get; init; } = 0;
    public decimal Credit { get; init; } = 0;

    public virtual bool Equals(CollationRow? other) =>
        Debet == other?.Debet && Credit == other.Credit && Date == other.Date;

    public override int GetHashCode()
    {
        return HashCode.Combine(Debet, Credit, Date);
    }

    private CollationRow(int serialNumber, DateTime date, decimal debet, decimal credit)
    {
        SerialNumber = serialNumber;
        Date = date;
        Debet = debet;
        Credit = credit;
    }

    public static Result<CollationRow, ErrorList> Create(
        int serialNumber, 
        DateTime date, 
        decimal debet, 
        decimal credit)
    {
        var errors = new List<Error>();
        
        if (serialNumber < 1)
            errors.Add(Errors.General.ValueIsInvalid(nameof(serialNumber)));
        
        if (date <= DateTime.MinValue || date > DateTime.UtcNow)
            errors.Add(Errors.General.ValueIsInvalid(nameof(date)));

        if (errors.Any())
            return (ErrorList)errors;
        
        return new CollationRow(serialNumber, date, debet, credit);
    }
}