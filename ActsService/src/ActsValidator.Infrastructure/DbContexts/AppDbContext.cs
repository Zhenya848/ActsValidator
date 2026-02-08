using ActsValidator.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ActsValidator.Infrastructure.DbContexts;

public class AppDbContext(IConfiguration configuration)  : DbContext
{
    public DbSet<Collation> Collations => Set<Collation>();
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("Database"));
        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.UseLoggerFactory(CreateLoggerFactory());
        optionsBuilder.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly); 

    private ILoggerFactory CreateLoggerFactory() =>
        LoggerFactory.Create(b => b.AddConsole());
}