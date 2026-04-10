using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PaymentMessaging.Contracts.Messaging;
using UserService.Application.Abstractions;
using UserService.Domain;
using UserService.Domain.Shared;
using UserService.Domain.Shared.Payment;
using static System.String;

namespace UserService.Infrastructure.Consumers;

public class ProductWasBoughtConsumer : IConsumer<ProductWasBoughtEvent>
{
    private readonly ILogger<ProductWasBoughtConsumer> _logger;
    private readonly UserManager<User> _userManager;
    private readonly Products _products;
    private readonly IUnitOfWork _unitOfWork;

    public ProductWasBoughtConsumer(
        ILogger<ProductWasBoughtConsumer> logger, 
        UserManager<User> userManager,
        Products products,
        IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _userManager = userManager;
        _products = products;
        _unitOfWork = unitOfWork;
    }
    
    public async Task Consume(ConsumeContext<ProductWasBoughtEvent> context)
    {
        var user = await _userManager.FindByIdAsync(context.Message.UserId.ToString());

        if (user is null)
        {
            _logger.LogCritical("User {id} not found", $"{context.Message.UserId}");
            return;
        }
        
        var productData = _products.Data.FirstOrDefault(x => x.ProductId == context.Message.ProductId);

        if (productData is null)
        {
            _logger.LogCritical("ProductData with id {id} not found", $"{context.Message.ProductId}");
            return;
        }

        user.UserAccess.TopUpBalance(productData.Amount);

        if (productData.Months > 0)
        {
            var subscribeResult = user.UserAccess.Subscribe(productData.Months);
        
            if (subscribeResult.IsFailure)
            {
                _logger.LogCritical(Join(", ", subscribeResult.Error.Select(e => $"{e.Code}: {e.Message}")));
                return;
            }
        }

        await _unitOfWork.SaveChanges(context.CancellationToken);
    }
}