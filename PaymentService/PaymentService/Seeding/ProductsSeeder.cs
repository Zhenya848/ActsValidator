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
            Product.Create(p.ProductId, p.Price))
            .ToArray();

        if (productsResults.Any(p => p.IsFailure))
        {
            logger.LogError($"Seeding {nameof(ProductData)} failed. Errors: " +
                            $"{productsResults.Select(e => e.Error.Message)}");
            
            throw new ApplicationException($"Seeding {nameof(ProductData)} was failed");
        }

        dbContext.Products.AttachRange(productsResults.Select(x => x.Value));

        await unitOfWork.SaveChanges();
    }
}