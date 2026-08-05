using MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PaymentMessaging.Contracts.Messaging;
using UserService.Application.Abstractions;
using UserService.Domain;
using UserService.Domain.Shared;
using UserService.Domain.Shared.Payment;
using UserService.Domain.User;
using UserService.Infrastructure.DbContexts;
using static System.String;

namespace UserService.Infrastructure.Consumers;

public class ProductWasBoughtConsumer : IConsumer<ProductWasBoughtEvent>
{
    private readonly AuthDbContext _dbContext;
    private readonly ILogger<ProductWasBoughtConsumer> _logger;
    private readonly UserManager<User> _userManager;
    private readonly Products _products;
    private readonly IUnitOfWork _unitOfWork;

    public ProductWasBoughtConsumer(
        AuthDbContext dbContext,
        ILogger<ProductWasBoughtConsumer> logger, 
        UserManager<User> userManager,
        Products products,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userManager = userManager;
        _products = products;
        _unitOfWork = unitOfWork;
    }
    
    public async Task Consume(ConsumeContext<ProductWasBoughtEvent> context)
    {
        var processedEvent = ProcessedEvent.Create(context.Message.Id, DateTime.UtcNow);

        if (processedEvent.IsFailure)
        {
            _logger.LogCritical(Join(", ", processedEvent.Error.Select(e => $"{e.Code}: {e.Message}")));
            return;
        }
        
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
        
        try
        {
            _dbContext.ProcessedEvents.Add(processedEvent.Value);
            
            user.UserAccess.TopUpBalance(productData.Amount);

            if (productData.Months > 0)
            {
                var subscribeResult = user.UserAccess.Subscribe(productData.Months);
        
                if (subscribeResult.IsFailure)
                {
                    _logger.LogCritical($"{subscribeResult.Error.Code}: {subscribeResult.Error.Message}");
                    return;
                }
            }

            await _unitOfWork.SaveChanges(context.CancellationToken);
        }
        catch (DbUpdateException)
        {
            _logger.LogWarning("Event {id} already processed", $"{context.Message.Id}");
        }
    }
}