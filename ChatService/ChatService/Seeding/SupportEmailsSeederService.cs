using ChatService.DbContexts;
using ChatService.Models.Email;
using ChatService.Options;

namespace ChatService.Seeding;

public class SupportEmailsSeederService(
    AppDbContext dbContext,
    ILogger<SupportEmailsSeederService> logger)
{
    public async Task SeedAsync(SupportEmailsOptions adminOptions)
    {
        try
        {
            for (int i = 0; i < adminOptions.Emails.Length; i++)
            {
                var supportEmailResult = SupportEmail.Create(adminOptions.Emails[i], i + 1);

                if (supportEmailResult.IsFailure)
                {
                    logger.LogCritical("Failed to create support email. Error: {error}",
                        $"{supportEmailResult.Error.Code}: {supportEmailResult.Error.Message}");

                    return;
                }

                dbContext.SupportEmails.Attach(supportEmailResult.Value);
            }
            
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to create support email.");
        }
    }
}