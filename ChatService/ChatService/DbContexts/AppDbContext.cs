using ChatService.Models.Chats;
using ChatService.Models.Email;
using ChatService.Models.Outbox;
using Microsoft.EntityFrameworkCore;

namespace ChatService.DbContexts;

public class AppDbContext(IConfiguration configuration) : DbContext
{
    public DbSet<Chat> Chats => Set<Chat>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<EmailDelivery> EmailDeliveries => Set<EmailDelivery>();
    
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