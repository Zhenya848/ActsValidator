using Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PaymentService.Abstractions;
using PaymentService.DbContexts;
using PaymentService.Models;
using PaymentService.Models.Shared.ValueObjects;

namespace PaymentService.Seeding;

public class ProductsSeeder(
    AppDbContext dbContext,
    IUnitOfWork unitOfWork,
    ILogger<ProductsSeeder> logger)
{
    public async Task SeedAsync()
    {
        var json = await File.ReadAllTextAsync("etc/products.json");

        var seedData = JsonConvert.DeserializeObject<ProductData[]>(json)
                       ?? throw new ApplicationException("Product Config is missing");

        var productsResults = seedData.Select(p =>
            Product.Create(p.ProductId, p.Price, p.Pack))
            .ToArray();

        if (productsResults.Any(p => p.IsFailure))
        {
            logger.LogError($"Seeding {nameof(ProductData)} failed. Errors: " +
                            $"{productsResults.Select(e => e.Error.Message)}");
            
            throw new ApplicationException($"Seeding {nameof(ProductData)} was failed");
        }

        var productsDict = productsResults.Select(p => p.Value).ToDictionary(p => p.Id);
        var existProducts = await dbContext.Products.ToListAsync();
        
        foreach (var existProduct in existProducts)
        {
            if (productsDict.TryGetValue(existProduct.Id, out var product) == false)
                dbContext.Products.Remove(existProduct);
            else
            {
                existProduct.Update(product.Price, product.Pack);
                productsDict.Remove(existProduct.Id);   
            }
        }
        
        dbContext.Products.AddRange(productsDict.Values);
        await unitOfWork.SaveChanges();
    }
}