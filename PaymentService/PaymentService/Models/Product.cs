using Core;
using CSharpFunctionalExtensions;
using PaymentService.Models.Shared;

namespace PaymentService.Models;

public class Product : Core.Entity<string>
{
    public int Price { get; private set; }
    
    private Product(string id) : base(id)
    {
        
    }

    private Product(string id, int price) : base(id)
    {
        Price = price;
    }

    public static Result<Product, Error> Create(string id, int price)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Errors.General.ValueIsRequired(nameof(id));
        
        if (price <= 0)
            return Errors.General.ValueIsInvalid(nameof(price));
        
        return new Product(id, price);
    }
    
    public UnitResult<Error> Update(int price)
    {
        if (price <= 0)
            return Errors.General.ValueIsInvalid(nameof(price));
        
        Price = price;
        
        return Result.Success<Error>();
    }
}