namespace UserService.Domain.Shared.Payment;

public class Products
{
    public const string PRODUCTS = "Products";
    public ProductData[] Data { get; }

    public Products(ProductData[] data)
    {
        Data = data;
    }
}