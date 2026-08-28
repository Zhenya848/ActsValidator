using ChatService.Abstractions;
using ChatService.DbContexts;
using ChatService.Models.Email;
using ChatService.Models.Shared;
using ChatService.Models.ValueObjects;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Providers;

public class SupportEmailsProvider
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger _logger;
    private List<SupportEmail> _supportEmails = [];
    public IReadOnlyList<SupportEmail> Emails => _supportEmails;

    public SupportEmailsProvider(IServiceScopeFactory scopeFactory, ILogger<SupportEmailsProvider> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        _supportEmails = await dbContext.SupportEmails
            .Where(s => s.Status == SupportEmailStatus.Available)
            .OrderBy(pn => pn.PriorityNumber)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task SetAsDisabled(IEnumerable<string> emails, CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        var selectedEmails = _supportEmails.Where(e => emails.Contains(e.Email)).ToArray();
        
        var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        foreach (var supportEmail in selectedEmails)
        {
            supportEmail.MarkAsDisabled();
            dbContext.SupportEmails.Update(supportEmail);
            
            _supportEmails.Remove(supportEmail);
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Could not save changes.");
            
            foreach (var supportEmail in selectedEmails)
                supportEmail.MarkAsAvailable();
            
            await transaction.RollbackAsync(cancellationToken);
        }
    }
}