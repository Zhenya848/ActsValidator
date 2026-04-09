using System.Text;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;

namespace ActsValidator.Domain.Shared.ValueObjects;

public record CollationRow : IEquatable<CollationRow>
{
    public int SerialNumber { get; init; } = 1;
    public DateTime Date { get; init; } = DateTime.Now;
    public decimal Debet { get; init; } = 0;
    public decimal Credit { get; init; } = 0;
    public int? DocumentNumber { get; init; }

    public virtual bool Equals(CollationRow? other) =>
        Debet == other?.Debet && Credit == other.Credit && Date.Date == other.Date.Date 
            && (other.DocumentNumber == null || DocumentNumber == null 
            || other.DocumentNumber.Value == DocumentNumber.Value);

    public override int GetHashCode()
    {
        return HashCode.Combine(Date, Debet, Credit, DocumentNumber);
    }

    private CollationRow(int serialNumber, DateTime date, decimal debet, decimal credit, int? documentNumber)
    {
        SerialNumber = serialNumber;
        Date = date;
        Debet = debet;
        Credit = credit;
        DocumentNumber = documentNumber;
    }

    public static Result<CollationRow, ErrorList> Create(
        int serialNumber, 
        DateTime date, 
        decimal debet, 
        decimal credit,
        string documentName)
    {
        var errors = new List<Error>();
        
        if (serialNumber < 1)
            errors.Add(Errors.General.ValueIsInvalid(nameof(serialNumber)));
        
        if (date <= DateTime.MinValue || date > DateTime.UtcNow)
            errors.Add(Errors.General.ValueIsInvalid(nameof(date)));

        if (errors.Any())
            return (ErrorList)errors;
        
        return new CollationRow(serialNumber, date, debet, credit, TryGetDocumentNumber(documentName));
    }
    
    public static Result<CollationRow, ErrorList> Create(
        int serialNumber, 
        DateTime date, 
        decimal debet, 
        decimal credit,
        int? documentNumber)
    {
        var errors = new List<Error>();
        
        if (serialNumber < 1)
            errors.Add(Errors.General.ValueIsInvalid(nameof(serialNumber)));
        
        if (date <= DateTime.MinValue || date > DateTime.UtcNow)
            errors.Add(Errors.General.ValueIsInvalid(nameof(date)));

        if (errors.Any())
            return (ErrorList)errors;
        
        return new CollationRow(serialNumber, date, debet, credit, documentNumber);
    }

    private static int? TryGetDocumentNumber(string documentName)
    {
        string pattern = @"(?<![\.\d])\b\d+\b(?![\.\d])";
        
        Match match = Regex.Match(documentName, pattern);
        
        if (match.Success && int.TryParse(match.Value, out var result))
            return result;
        
        return null;
    }
}