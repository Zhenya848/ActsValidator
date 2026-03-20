using Core;
using CSharpFunctionalExtensions;
using PaymentService.Models.Shared;

namespace PaymentService.Models;

public class Product : Core.Entity<string>
{
    public int Price { get; private set; }
    public int Pack { get; private set; }
    
    private Product(string id) : base(id)
    {
        
    }

    private Product(string id, int price, int pack) : base(id)
    {
        Price = price;
        Pack = pack;
    }

    public static Result<Product, Error> Create(string id, int price, int pack)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Errors.General.ValueIsRequired(nameof(id));
        
        if (price <= 0)
            return Errors.General.ValueIsInvalid(nameof(price));
        
        if (pack <= 0)
            return Errors.General.ValueIsInvalid(nameof(pack));
        
        return new Product(id, price, pack);
    }
    
    public UnitResult<Error> Update(int price, int pack)
    {
        if (price <= 0)
            return Errors.General.ValueIsInvalid(nameof(price));
        
        if (pack <= 0)
            return Errors.General.ValueIsInvalid(nameof(pack));
        
        Price = price;
        Pack = pack;
        
        return Result.Success<Error>();
    }
}