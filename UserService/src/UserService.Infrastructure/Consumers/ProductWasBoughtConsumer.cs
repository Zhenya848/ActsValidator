using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PaymentMessaging.Contracts.Messaging;
using UserService.Domain.Shared;

namespace UserService.Infrastructure.Consumers;

public class ProductWasBoughtConsumer : IConsumer<ProductWasBoughtEvent>
{
    private readonly ILogger<ProductWasBoughtConsumer> _logger;

    public ProductWasBoughtConsumer(ILogger<ProductWasBoughtConsumer> logger)
    {
        _logger = logger;
    }
    
    public async Task Consume(ConsumeContext<ProductWasBoughtEvent> context)
    {
        var json = await File.ReadAllTextAsync("etc/products.json");
        
        var seedData = JsonConvert.DeserializeObject<ProductData[]>(json)
                       ?? throw new ApplicationException("Product Config is missing");

        var productId = seedData.Select(p => p.ProductId).FirstOrDefault(context.Message.ProductId);
        
        if (productId.)
    }
}