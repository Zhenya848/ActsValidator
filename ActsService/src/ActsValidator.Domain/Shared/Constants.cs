namespace ActsValidator.Domain.Shared;

public static class Constants
{
    public static class DiscrepancyFields
    {
        public const string Date = "дата";
        public const string Debet = "дебет";
        public const string Credit = "кредит";
        public const string Document = "документ";
        public const string Missed = "отсутствует";
    
        public static readonly string[] RequiredCells = [Date, Credit, Debet, Document];
    }

    public static class DiscrepancySeverity
    {
        public const string Low = "low";
        public const string Medium = "medium";
        public const string High = "high";
    }
}