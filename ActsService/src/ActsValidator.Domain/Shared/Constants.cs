namespace ActsValidator.Domain.Shared;

public static class Constants
{
    public const string Date = "дата";
    public const string Debet = "дебет";
    public const string Credit = "кредит";
    
    public static readonly string[] RequiredCells = [Date, Credit, Debet];
}