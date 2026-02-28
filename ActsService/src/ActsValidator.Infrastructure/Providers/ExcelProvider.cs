using System.Globalization;
using ActsValidator.Application.Providers;
using ActsValidator.Domain.Shared;
using ActsValidator.Domain.Shared.ValueObjects;
using ClosedXML.Excel;
using CSharpFunctionalExtensions;

namespace ActsValidator.Infrastructure.Providers;

public class ExcelProvider : IFileProvider
{
    private Result<Dictionary<string, int>, ErrorList> GetHeaders(IXLWorksheet worksheet)
    {
        var searchRange = worksheet.Range(1, 1, 20, 20);
        
        var results = Constants.RequiredCells
            .Select(name => new
            {
                Name = name,
                Cell = searchRange.CellsUsed(c => c.Value.ToString().Trim().ToLower() == name)
                    .FirstOrDefault()
            })
            .ToList();
        
        var errors = results
            .Where(x => x.Cell == null)
            .Select(x => Error.NotFound("header.not.found", $"Header {x.Name} is not found"))
            .ToList();

        if (errors.Count > 0)
            return (ErrorList)errors;

        return results.ToDictionary(x => x.Name, x => x.Cell!.Address.ColumnNumber);
    }

    public Result<IEnumerable<CollationRow>, ErrorList> GetCollationRows(Stream file, bool reverse = false)
    {
        try
        {
            using var excelWorkbook = new XLWorkbook(file);
            var worksheet = excelWorkbook.Worksheet(1);
        
            if (worksheet is null)
                return (ErrorList)Error.NotFound("worksheet.not.found", "Worksheet is not found");

            var headersResult = GetHeaders(worksheet);
        
            if (headersResult.IsFailure)
                return headersResult.Error;

            var headers = headersResult.Value;
        
            var results = new List<CollationRow>();
        
            var startIndex = worksheet.CellsUsed(c => 
                    Constants.RequiredCells.Contains(c.Value.ToString().Trim().ToLower()))
                .First()
                .Address
                .RowNumber;

            for (int i = startIndex + 1; i < worksheet.LastRowUsed()!.RowNumber(); i++)
            {
                var currentRow = worksheet.Row(i);
            
                var date = ReturnDate(currentRow.Cell(headers[Constants.Date]));
                var debet = ReturnDecimal(currentRow.Cell(headers[Constants.Debet]));
                var credit = ReturnDecimal(currentRow.Cell(headers[Constants.Credit]));
            
                if (date is null)
                    continue;
                
                var isNegative = debet < 0 || credit < 0;

                if ((isNegative && reverse == false) || (isNegative == false && reverse))
                    (debet, credit) = (credit, debet);
                
                (debet, credit) = (Math.Abs(debet), Math.Abs(credit));
            
                var collationRowResult = CollationRow.Create(i, date.Value, debet, credit);
            
                if (collationRowResult.IsFailure)
                    return collationRowResult.Error;
            
                results.Add(collationRowResult.Value);
            }
        
            return results;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
    
    private DateTime? ReturnDate(IXLCell cell)
    {
        if (cell.Value.IsDateTime)
            return DateTime.SpecifyKind(cell.Value.GetDateTime(), DateTimeKind.Utc);
        
        var textValue = cell.Value.ToString().Trim();
        
        if (string.IsNullOrWhiteSpace(textValue)) 
            return null;

        string[] formats = {
            "dd.MM.yyyy HH:mm:ss", "dd.MM.yyyy", "dd.MM.yy",
            "d.M.yyyy", "d.M.yy",
            "yyyy-MM-dd", "dd/MM/yyyy"
        };

        if (DateTime.TryParseExact(textValue, formats, CultureInfo.InvariantCulture, 
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
        {
            return date;
        }

        return null;
    }

    private decimal ReturnDecimal(IXLCell cell)
    {
        var value = cell.Value.ToString();

        if (decimal.TryParse(value, out var decimalValue) == false)
            return 0;
        
        return decimalValue;
    }
}