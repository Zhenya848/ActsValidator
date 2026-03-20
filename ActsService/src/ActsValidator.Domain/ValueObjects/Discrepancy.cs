using System.Globalization;
using System.Numerics;
using System.Text.Json.Serialization;
using ActsValidator.Domain.Shared;
using ActsValidator.Domain.Shared.ValueObjects;
using CSharpFunctionalExtensions;

namespace ActsValidator.Domain.ValueObjects;

public record Discrepancy
{
    public int? Act1Row { get; init; }
    public int? Act2Row { get; init; }
    public string Act1Value { get; init; }
    public string Act2Value { get; init; }
    public string Difference { get; init; }
    public string Field { get; init; }
    public string Severity { get; init; }
    public HashSet<string> DetectedBy { get; init; } = [];

    [JsonConstructor]
    private Discrepancy(int? act1Row, int? act2Row, string act1Value, string act2Value, 
        string field, string difference, string severity, IEnumerable<string> detectedBy)
    {
        Act1Row = act1Row;
        Act2Row = act2Row;
        Act1Value = act1Value;
        Act2Value = act2Value;
        Field = field;
        Difference = difference;
        Severity = severity;
        DetectedBy = detectedBy.ToHashSet();
    }
    
    private Discrepancy(int? act1Row, int? act2Row, string act1Value, string act2Value, 
        string field, string difference, string severity)
    {
        Act1Row = act1Row;
        Act2Row = act2Row;
        Act1Value = act1Value;
        Act2Value = act2Value;
        Field = field;
        Difference = difference;
        Severity = severity;
    }

    public static Result<Discrepancy, ErrorList> Create(CollationRow? act1, CollationRow? act2)
    {
        if (act1 is null && act2 is null)
            return (ErrorList)Error.Failure(
                "discrepancy.create.failure", 
                "Unable to create discrepancies because only 1 or 0 act can be null");
        
        if (act1 != null && act1.Equals(act2))
            return (ErrorList)Error.Failure(
                "discrepancy.create.failure", 
                "Unable to create discrepancies because act1 and act2 have common rows");
        
        if (act1 is null || act2 is null)
            return new Discrepancy(
                act1?.SerialNumber, 
                act2?.SerialNumber, 
                act1 is not null ? act1.Debet.ToString(CultureInfo.InvariantCulture) : string.Empty,
                act2 is not null ? act2.Credit.ToString(CultureInfo.InvariantCulture) : string.Empty,
                Constants.DiscrepancyFields.Missed, 
                "none",
                Constants.DiscrepancySeverity.High);
        
        return GetWithDifference(act1, act2);
    }

    public static Result<Discrepancy, ErrorList> Create(
        int? act1Row, 
        int? act2Row, 
        string act1Value,
        string act2Value,
        string field, 
        string difference,
        string severity)
    {
        var errors = new List<Error>();
        
        if (act1Row < 1)
            errors.Add(Errors.General.ValueIsInvalid(nameof(act1Row)));
        
        if (act2Row < 1)
            errors.Add(Errors.General.ValueIsInvalid(nameof(act2Row)));
        
        if (string.IsNullOrWhiteSpace(field))
            errors.Add(Errors.General.ValueIsRequired(nameof(field)));
        
        if (string.IsNullOrWhiteSpace(difference))
            errors.Add(Errors.General.ValueIsRequired(nameof(difference)));
        
        if (errors.Count > 0)
            return (ErrorList)errors;
        
        return new Discrepancy(act1Row, act2Row, act1Value, act2Value, field, difference, severity);
    }

    private static Discrepancy GetWithDifference(CollationRow act1, CollationRow act2)
    {
        if (act1.Date.Equals(act2.Date) == false)
        {
            return new Discrepancy(
                act1.SerialNumber, 
                act2.SerialNumber, 
                act1.Date.ToString(CultureInfo.InvariantCulture),
                act2.Date.ToString(CultureInfo.InvariantCulture),
                Constants.DiscrepancyFields.Date, 
                GetDateDifference(act1.Date, act2.Date),
                GetSeverity(act1.Date, act2.Date, (x, y) => (x - y).TotalMinutes));
        }

        return act1.Debet.Equals(act2.Credit) == false
            ? new Discrepancy(
                act1.SerialNumber,
                act2.SerialNumber,
                act1.Debet.ToString(CultureInfo.InvariantCulture),
                act2.Credit.ToString(CultureInfo.InvariantCulture),
                Constants.DiscrepancyFields.Debet,
                Math.Abs(act1.Debet - act2.Credit).ToString(CultureInfo.InvariantCulture),
                GetSeverity(act1.Debet, act2.Credit, (x, y) => (double)(x - y)))
            : new Discrepancy(
                act1.SerialNumber,
                act2.SerialNumber,
                act1.Credit.ToString(CultureInfo.InvariantCulture),
                act2.Debet.ToString(CultureInfo.InvariantCulture),
                Constants.DiscrepancyFields.Credit,
                Math.Abs(act1.Credit - act2.Debet).ToString(CultureInfo.InvariantCulture),
                GetSeverity(act1.Credit, act2.Debet, (x, y) => (double)(x - y)));
    }
    
    private static string GetDateDifference(DateTime start, DateTime end)
    {
        if (end < start)
            (start, end) = (end, start);

        var years = end.Year - start.Year;
        var months = end.Month - start.Month;
        var days = end.Day - start.Day;

        if (days < 0)
        {
            months--;
            var prevMonth = end.AddMonths(-1);
            days += DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
        }

        if (months < 0)
        {
            years--;
            months += 12;
        }

        return $"{years} года {months} месяцев {days} дней";
    }
    
    private static string GetSeverity<T>(
        T a,
        T b,
        Func<T, T, double> diffFunc)
    {
        var diff = Math.Abs(diffFunc(a, b));

        return diff switch
        {
            < 1000 => Constants.DiscrepancySeverity.Low,
            < 10000 => Constants.DiscrepancySeverity.Medium,
            _ => Constants.DiscrepancySeverity.High
        };
    }

    public UnitResult<ErrorList> AddDetectedCharacter(params IEnumerable<string> detectedCharacters)
    {
        foreach (var detectedCharacter in detectedCharacters)
        {
            if (DetectedBy.Add(detectedCharacter) == false)
                return (ErrorList)Errors.General.ValueIsInvalid(nameof(detectedCharacter));
        }

        return Result.Success<ErrorList>();
    }
}