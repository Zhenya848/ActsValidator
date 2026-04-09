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
    
    record CollationValueDictionary(int SerialNumber, int Count);
    
    private static Result<CollationResult, ErrorList> Compare(
        List<CollationRow> act1,
        List<CollationRow> reversedAct2)
    {
        UnitResult<ErrorList> MakePairsBy(
            Func<CollationRow, object?> selector,
            List<CollationRow> diff1,
            List<CollationRow> diff2,
            HashSet<Discrepancy> totalDiscrepancies,
            HashSet<int> usedRowsSerialNumbersInAct2)
        {
            var diff2BySelector = diff2.ToLookup(selector);

            for (var i = 0; i < diff1.Count; i++)
            {
                var row2BySelector = diff2BySelector[selector(diff1[i])]
                    .FirstOrDefault(r => usedRowsSerialNumbersInAct2.Contains(r.SerialNumber) == false);
            
                if (row2BySelector is null) 
                    continue;
            
                var normalizedRow2 = CollationRow
                    .Create(
                        row2BySelector.SerialNumber, 
                        row2BySelector.Date, 
                        row2BySelector.Credit, 
                        row2BySelector.Debet, 
                        row2BySelector.DocumentNumber)
                    .Value;
            
                var discrepancyResult = Discrepancy.Create(diff1[i], normalizedRow2);
            
                if (discrepancyResult.IsFailure)
                    return discrepancyResult.Error;

                discrepancyResult.Value.AddDetectedCharacter(Constants.DetectedBy.Algorythm);
                
                usedRowsSerialNumbersInAct2.Add(row2BySelector.SerialNumber);
                totalDiscrepancies.Add(discrepancyResult.Value);
                diff1.Remove(diff1[i]);
                i--;
            }

            return Result.Success<ErrorList>();
        }

        act1 = act1.GroupBy(x => x.DocumentNumber).Select(x =>
        {
            var serialNumber = x.First().SerialNumber;
            var date = x.First().Date;
            var debet = x.Sum(y => y.Debet);
            var credit = x.Sum(y => y.Credit);
            
            if (debet != 0 && credit != 0)
                return debet >= credit 
                    ? CollationRow.Create(serialNumber, date, debet - credit, 0, x.Key).Value
                    : CollationRow.Create(serialNumber, date, 0, credit - debet, x.Key).Value;

            return CollationRow.Create(serialNumber, date, debet, credit, x.Key).Value;
        }).ToList();
        
        reversedAct2 = reversedAct2.GroupBy(x => x.DocumentNumber).Select(x =>
        {
            var serialNumber = x.First().SerialNumber;
            var date = x.First().Date;
            var debet = x.Sum(y => y.Debet);
            var credit = x.Sum(y => y.Credit);
            
            if (debet != 0 && credit != 0)
                return debet >= credit 
                    ? CollationRow.Create(serialNumber, date, debet - credit, 0, x.Key).Value
                    : CollationRow.Create(serialNumber, date, 0, credit - debet, x.Key).Value;

            return CollationRow.Create(serialNumber, date, debet, credit, x.Key).Value;
        }).ToList();
        
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
                if (c1 - c2 > 1)
                    diff1.AddRange(act1.Where(x => x == key).Take(c1 - c2));
                else
                    diff1.Add(key);
            }
            else if (c2 > c1)
            {
                if (c2 - c1 > 1)
                    diff2.AddRange(reversedAct2.Where(x => x == key).Take(c2 - c1));
                else
                    diff2.Add(key);
            }
        }
        
        foreach (var kvp in counts2.Where(kvp => counts1.ContainsKey(kvp.Key) == false))
        {
            if (kvp.Value > 1)
                diff2.AddRange(reversedAct2.Where(x => x == kvp.Key).Take(kvp.Value));
            else
                diff2.Add(kvp.Key);
        }
        
        var totalDiscrepancies = new HashSet<Discrepancy>();
        var coincidencesCount = (act1.Count + reversedAct2.Count - diff1.Count - diff2.Count) / 2;
        var usedRowsSerialNumbersInAct2 = new HashSet<int>();

        var pairsByDocumentNumberResult = MakePairsBy(x => x.DocumentNumber, diff1, diff2, totalDiscrepancies, 
            usedRowsSerialNumbersInAct2);
        
        if (pairsByDocumentNumberResult.IsFailure)
            return pairsByDocumentNumberResult.Error;
        
        var pairsByDateResult = MakePairsBy(x => x.Date, diff1, diff2, totalDiscrepancies, 
            usedRowsSerialNumbersInAct2);
        
        if (pairsByDateResult.IsFailure)
            return pairsByDateResult.Error;
        
        var pairsByAmountResult = MakePairsBy(x => (x.Debet, x.Credit), diff1, diff2, totalDiscrepancies, 
            usedRowsSerialNumbersInAct2);
        
        if (pairsByAmountResult.IsFailure)
            return pairsByAmountResult.Error;
        
        var diff2WithNoPair = diff2
            .Where(d => usedRowsSerialNumbersInAct2.Contains(d.SerialNumber) == false)
            .ToArray();

        if (diff2WithNoPair.Length < 1 && diff1.Count < 1) 
            return new CollationResult(totalDiscrepancies, coincidencesCount);
        
        var discrepancies1 = diff1
            .Select(x =>
            {
                var discrepancyResult = Discrepancy.Create(x, null);
                
                if (discrepancyResult.IsSuccess)
                    discrepancyResult.Value.AddDetectedCharacter(Constants.DetectedBy.Algorythm);

                return discrepancyResult;
            })
            .ToArray();
        
        var discrepancies2 = diff2WithNoPair
            .Select(x =>
            {
                var discrepancyResult = Discrepancy.Create(null, x);
                
                if (discrepancyResult.IsSuccess)
                    discrepancyResult.Value.AddDetectedCharacter(Constants.DetectedBy.Algorythm);

                return discrepancyResult;
            })
            .ToArray();
            
        if (discrepancies1.Any(x => x.IsFailure))
            return (ErrorList)discrepancies1.SelectMany(x => x.Error).ToList();
        
        if (discrepancies2.Any(x => x.IsFailure))
            return (ErrorList)discrepancies2.SelectMany(x => x.Error).ToList();

        foreach (var discrepancy in discrepancies1)
            totalDiscrepancies.Add(discrepancy.Value);
        
        foreach (var discrepancy in discrepancies2)
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