using ActsValidator.Application.Repositories;
using ActsValidator.Domain;
using ActsValidator.Infrastructure.DbContexts;
using Microsoft.Extensions.Logging;

namespace ActsValidator.Infrastructure.Repositories;

public class CollationRepository : ICollationRepository
{
    private readonly AppDbContext  _dbContext;
    private readonly ILogger<CollationRepository> _logger;

    public CollationRepository(
        AppDbContext dbContext,  
        ILogger<CollationRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    
    public Guid Add(Collation collation)
    {
        _dbContext.Collations.Add(collation);
        _logger.LogInformation("Added Collation {collation} with id {collationId}", collation, collation.Id.Value);
        
        return collation.Id;
    }
}