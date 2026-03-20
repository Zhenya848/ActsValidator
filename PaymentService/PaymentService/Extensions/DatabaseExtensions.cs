using Microsoft.EntityFrameworkCore;
using PaymentService.DbContexts;

namespace PaymentService.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrations(this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        await dbContext.Database.MigrateAsync();
    }
}