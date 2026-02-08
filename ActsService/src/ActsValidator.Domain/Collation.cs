using ActsValidator.Domain.Shared;
using ActsValidator.Domain.Shared.ValueObjects;
using ActsValidator.Domain.Shared.ValueObjects.Ids;
using ActsValidator.Domain.ValueObjects;
using CSharpFunctionalExtensions;

namespace ActsValidator.Domain;

public class Collation : Shared.Entity<CollationId>
{
    public List<CollationRow> Act1 { get; } = [];
    public List<CollationRow> Act2 { get; } = [];

    public List<Discrepancy> Discrepancies { get; } = [];
    
    private Collation(CollationId id) : base(id)
    {
        
    }

    private Collation(
        CollationId id, 
        List<CollationRow> act1, 
        List<CollationRow> act2,
        List<Discrepancy> discrepancies) : base(id)
    {
        Act1 = act1;
        Act2 = act2;
        
        Discrepancies = discrepancies;
    }

    private static Result<List<Discrepancy>, ErrorList> GetDiscrepancies(
        List<CollationRow> act1,
        List<CollationRow> act2)
    {
        var counts1 = act1.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());
        var counts2 = act2.GroupBy(x => x).ToDictionary(g => g.Key, g => g.Count());

        var diff1 = new List<CollationRow>();
        var diff2 = new List<CollationRow>();
        
        var allKeys = counts1.Keys.Union(counts2.Keys);

        foreach (var key in allKeys)
        {
            counts1.TryGetValue(key, out var c1);
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
        
        var diff2ByDate = diff2.ToLookup(r => r.Date);
        var diff2ByAmount = diff2.ToLookup(r => (r.Debet, r.Credit));
        
        var result = new List<Discrepancy>();
        var usedRowsSerialNumbers = new HashSet<int>();

        foreach (var row in diff1)
        {
            var row2ByDate = diff2ByDate[row.Date]
                .FirstOrDefault(r => usedRowsSerialNumbers.Contains(r.SerialNumber) == false);

            if (row2ByDate != null)
            {
                var discrepancyResult = Discrepancy.Create(
                    row, 
                    row2ByDate, 
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
                
                var discrepancyResult = Discrepancy.Create(row, row2ByAmount, Constants.Date);
                
                if (discrepancyResult.IsFailure)
                    return discrepancyResult.Error;
                
                if (row2ByAmount != null)
                    usedRowsSerialNumbers.Add(row2ByAmount.SerialNumber);
                
                result.Add(discrepancyResult.Value);
            }
        }
        
        return result;
    }

    public static Result<Collation, ErrorList> Create(
        IEnumerable<CollationRow> act1,
        IEnumerable<CollationRow> act2)
    {
        var act1List = act1.ToList();
        var act2List = act2.ToList();
        
        var discrepanciesResult = GetDiscrepancies(act1List, act2List);
        
        if (discrepanciesResult.IsFailure) 
            return discrepanciesResult.Error;
        
        return new Collation(CollationId.AddNewId(), act1List, act2List, discrepanciesResult.Value);
    }
}