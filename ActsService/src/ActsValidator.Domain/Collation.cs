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

    public List<Discrepancy> Discrepancies { get; } = [];
    
    public AiRequest? AiRequest { get; }
    
    private Collation(CollationId id) : base(id)
    {
        
    }

    private Collation(
        CollationId id, 
        Guid userId,
        string act1Name, 
        string act2Name,
        List<Discrepancy> discrepancies) : base(id)
    {
        UserId = userId;
        
        Act1Name = act1Name;
        Act2Name = act2Name;
        
        Discrepancies = discrepancies;
    }

    private static Result<List<Discrepancy>, ErrorList> GetDiscrepancies(
        List<CollationRow> act1,
        List<CollationRow> act2)
    {
        var reversedAct2 = act2.Select(x => CollationRow
                .Create(x.SerialNumber, x.Date, Math.Abs(x.Credit), Math.Abs(x.Debet)).Value)
        .ToList();
        
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
        
        var result = new List<Discrepancy>();
        var usedRowsSerialNumbers = new HashSet<int>();

        if (diff1.Count == 0)
        {
            var discrepancies = diff2.Select(x => Discrepancy
                .Create(null, x, Constants.Date)).ToList();

            if (discrepancies.Any(x => x.IsFailure))
                return (ErrorList)discrepancies.SelectMany(x => x.Error).ToList();
            
            return discrepancies.Select(x => x.Value).ToList();
        }

        foreach (var row in diff1)
        {
            var row2ByDate = diff2ByDate[row.Date]
                .FirstOrDefault(r => usedRowsSerialNumbers.Contains(r.SerialNumber) == false);

            if (row2ByDate != null)
            {
                var normalizedRow2 = CollationRow
                    .Create(row2ByDate.SerialNumber, row2ByDate.Date, row2ByDate.Credit, row2ByDate.Debet)
                    .Value;
                
                var discrepancyResult = Discrepancy.Create(
                    row, 
                    normalizedRow2, 
                    row.Debet != row2ByDate.Debet ? Constants.Debet : Constants.Credit);
            
                if (discrepancyResult.IsFailure)
                    return discrepancyResult.Error;
                
                usedRowsSerialNumbers.Add(row2ByDate.SerialNumber);
                result.Add(discrepancyResult.Value);
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
                
                var discrepancyResult = Discrepancy.Create(row, normalizedRow2, Constants.Date);
                
                if (discrepancyResult.IsFailure)
                    return discrepancyResult.Error;
                
                if (normalizedRow2 != null)
                    usedRowsSerialNumbers.Add(normalizedRow2.SerialNumber);
                
                result.Add(discrepancyResult.Value);
            }
        }
        
        return result;
    }

    public static Result<Collation, ErrorList> Create(
        Guid userId,
        string act1Name,
        string act2Name,
        IEnumerable<CollationRow> act1,
        IEnumerable<CollationRow> act2)
    {
        var errors = new List<Error>();
        
        var act1List = act1.ToList();
        var act2List = act2.ToList();
        
        if (string.IsNullOrWhiteSpace(act1Name))
            errors.Add(Errors.General.ValueIsRequired(nameof(act1Name)));
        
        if (string.IsNullOrWhiteSpace(act2Name))
            errors.Add(Errors.General.ValueIsRequired(nameof(act2Name)));
        
        var discrepanciesResult = GetDiscrepancies(act1List, act2List);
        
        if (discrepanciesResult.IsFailure) 
            errors.AddRange(discrepanciesResult.Error);

        if (errors.Count > 0)
            return (ErrorList)errors;
        
        return new Collation(CollationId.AddNewId(), userId, act1Name, act2Name, discrepanciesResult.Value);
    }
}