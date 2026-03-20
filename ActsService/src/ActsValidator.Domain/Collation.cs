using System.Xml.Schema;
using ActsValidator.Domain.Shared;
using ActsValidator.Domain.Shared.ValueObjects;
using ActsValidator.Domain.Shared.ValueObjects.Ids;
using ActsValidator.Domain.ValueObjects;
using CSharpFunctionalExtensions;

namespace ActsValidator.Domain;

public class Collation : Shared.Entity<CollationId>
{
    public Guid UserId { get; }
    
    public string Act1Name { get; }
    public string Act2Name { get; }

    public int CoincidencesCount { get; }
    public int RowsProcessed { get; }
    
    public HashSet<Discrepancy> CollationErrors { get; } = [];
    public CollationStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    
    private Collation(CollationId id) : base(id)
    {
        
    }

    private Collation(
        CollationId id, 
        Guid userId,
        string act1Name, 
        string act2Name,
        int coincidencesCount,
        int rowsProcessed,
        HashSet<Discrepancy> collationErrors,
        DateTime createdAt) : base(id)
    {
        UserId = userId;
        
        Act1Name = act1Name;
        Act2Name = act2Name;
        CoincidencesCount = coincidencesCount;
        RowsProcessed = rowsProcessed;
        
        CollationErrors = collationErrors;
        Status = collationErrors.Count switch
        {
            < 1 => CollationStatus.Success,
            < 10 => CollationStatus.Warning,
            _ => CollationStatus.Error
        };
        
        CreatedAt = createdAt;
    }
    
    private static Result<CollationResult, ErrorList> Compare(
        List<CollationRow> act1,
        List<CollationRow> reversedAct2)
    {
        var counts1 = act1.GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());
        
        var counts2 = reversedAct2.GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());

        var diff1 = new List<CollationRow>();
        var diff2 = new List<CollationRow>();
        
        foreach (var (key, c1) in counts1)
        {
            counts2.TryGetValue(key, out var c2);

            if (c1 > c2)
            {
                for (int i = 0; i < (c1 - c2); i++) 
                    diff1.Add(key);
            }
            else if (c2 > c1)
            {
                for (int i = 0; i < (c2 - c1); i++) 
                    diff2.Add(key);
            }
        }

        foreach (var kvp in counts2.Where(kvp => counts1.ContainsKey(kvp.Key) == false))
        {
            for (int i = 0; i < kvp.Value; i++) 
                diff2.Add(kvp.Key);
        }
        
        var diff2ByDate = diff2.ToLookup(r => r.Date);
        var diff2ByAmount = diff2.ToLookup(r => (r.Debet, r.Credit));
        
        var totalDiscrepancies = new HashSet<Discrepancy>();
        
        var coincidencesCount = (act1.Count + reversedAct2.Count - diff1.Count - diff2.Count) / 2;
        
        var usedRowsSerialNumbers = new HashSet<int>();

        foreach (var row in diff1)
        {
            var row2ByDate = diff2ByDate[row.Date]
                .FirstOrDefault(r => usedRowsSerialNumbers.Contains(r.SerialNumber) == false);

            if (row2ByDate != null)
            {
                var normalizedRow2 = CollationRow
                    .Create(row2ByDate.SerialNumber, row2ByDate.Date, row2ByDate.Credit, row2ByDate.Debet)
                    .Value;
                
                var discrepancyResult = Discrepancy.Create(row, normalizedRow2);
            
                if (discrepancyResult.IsFailure)
                    return discrepancyResult.Error;

                discrepancyResult.Value.AddDetectedCharacter(Constants.DetectedBy.Ai);
                
                usedRowsSerialNumbers.Add(row2ByDate.SerialNumber);
                totalDiscrepancies.Add(discrepancyResult.Value);
            }
            else
            {
                var row2ByAmount = diff2ByAmount[(row.Debet, row.Credit)]
                    .FirstOrDefault(r => usedRowsSerialNumbers.Contains(r.SerialNumber) == false);
                
                var normalizedRow2 = row2ByAmount is not null 
                    ? 
                    CollationRow.Create(
                            row2ByAmount.SerialNumber, 
                            row2ByAmount.Date, 
                            row2ByAmount.Credit, 
                            row2ByAmount.Debet).Value 
                    : null;
                
                var discrepancyResult = Discrepancy.Create(row, normalizedRow2);
                
                if (discrepancyResult.IsFailure)
                    return discrepancyResult.Error;
                
                if (normalizedRow2 != null)
                    usedRowsSerialNumbers.Add(normalizedRow2.SerialNumber);
                
                discrepancyResult.Value.AddDetectedCharacter(Constants.DetectedBy.Ai);
                
                totalDiscrepancies.Add(discrepancyResult.Value);
            }
        }

        var diff2WithNoPair = diff2
            .Where(d => usedRowsSerialNumbers.Contains(d.SerialNumber) == false)
            .ToArray();

        if (diff2WithNoPair.Length < 1) 
            return new CollationResult(totalDiscrepancies, coincidencesCount);
        
        var discrepancies = diff2WithNoPair
            .Select(x =>
            {
                var discrepancyResult = Discrepancy.Create(null, x);
                
                if (discrepancyResult.IsSuccess)
                    discrepancyResult.Value.AddDetectedCharacter(Constants.DetectedBy.Ai);

                return discrepancyResult;
            })
            .ToArray();
            
        if (discrepancies.Any(x => x.IsFailure))
            return (ErrorList)discrepancies.SelectMany(x => x.Error).ToList();

        foreach (var discrepancy in discrepancies)
            totalDiscrepancies.Add(discrepancy.Value);
        
        return new CollationResult(totalDiscrepancies, coincidencesCount);
    }

    public static Result<Collation, ErrorList> Create(
        Guid userId,
        string act1Name,
        string act2Name,
        IEnumerable<CollationRow> act1,
        IEnumerable<CollationRow> reversedAct2)
    {
        var errors = new List<Error>();
        
        var act1List = act1.ToList();
        var act2List = reversedAct2.ToList();
        
        if (string.IsNullOrWhiteSpace(act1Name))
            errors.Add(Errors.General.ValueIsRequired(nameof(act1Name)));
        
        if (string.IsNullOrWhiteSpace(act2Name))
            errors.Add(Errors.General.ValueIsRequired(nameof(act2Name)));
        
        var collationResult = Compare(act1List, act2List);
        
        if (collationResult.IsFailure) 
            errors.AddRange(collationResult.Error);

        if (errors.Count > 0)
            return (ErrorList)errors;
        
        return new Collation(
            CollationId.AddNewId(), 
            userId, 
            act1Name, 
            act2Name, 
            collationResult.Value.CoincidencesCount, 
            act1List.Count + act2List.Count,
            collationResult.Value.Errors, 
            DateTime.UtcNow);
    }
}