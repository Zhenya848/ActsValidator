using ActsValidator.Application.Repositories;
using ActsValidator.Domain;
using ActsValidator.Infrastructure.DbContexts;
using Microsoft.Extensions.Logging;

namespace ActsValidator.Infrastructure.Repositories;

public class AppRepository : IAppRepository
{
    private readonly AppDbContext  _dbContext;
    private readonly ILogger<AppRepository> _logger;

    public AppRepository(
        AppDbContext dbContext,  
        ILogger<AppRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public Guid AddCollation(Collation collation)
    {
        _dbContext.Collations.Add(collation);
        _logger.LogInformation("Added Collation {collation} with id {collationId}", collation, collation.Id.Value);
        
        return collation.Id;
    }

    public Guid AddAiRequest(AiRequest aiRequest)
    {
        _dbContext.AiRequests.Add(aiRequest);
        _logger.LogInformation("Added AiRequest {aiRequest} with id {aiRequestId}", aiRequest, aiRequest.Id.Value);
        
        return aiRequest.Id;
    }
}