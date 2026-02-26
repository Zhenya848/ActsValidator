namespace ActsValidator.Domain.Shared.ValueObjects.Dtos;

public record CollationRowDto
{
    public int SerialNumber { get; init; } = 1;
    public DateTime Date { get; init; } = DateTime.Now;
    public decimal Debet { get; init; } = 0;
    public decimal Credit { get; init; } = 0;

    public CollationRowDto(int serialNumber, DateTime date, decimal debet, decimal credit)
    {
        SerialNumber = serialNumber;
        Date = date;
        Debet = debet;
        Credit = credit;
    }
}