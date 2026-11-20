using ChatService.DbContexts;
using ChatService.EmailSender;
using ChatService.Models.Email;
using ChatService.Options;
using Microsoft.EntityFrameworkCore;

namespace ChatService.Seeding;

public class SupportEmailsSeederService(
    AppDbContext dbContext,
    ILogger<SupportEmailsSeederService> logger)
{
    public async Task SeedAsync(SupportEmailsOptions adminOptions)
    {
        try
        {
            var emails = adminOptions.Emails.Select(SupportEmail.Create).ToArray();

            if (emails.Any(x => x.IsFailure))
            {
                logger.LogError($"Seeding {nameof(SupportEmail)} failed. " +
                                $"{string.Join(", ", emails.Select(e => $"{e.Error.Code}: {e.Error.Message}"))}");
            
                throw new ApplicationException($"Seeding {nameof(SupportEmail)} was failed");
            }
            
            var emailsDict = emails.Select(x => x.Value).ToDictionary(x => x.Id);
            var existsEmails = await dbContext.SupportEmails.ToListAsync();

            foreach (var existEmail in existsEmails)
            {
                if (emailsDict.TryGetValue(existEmail.Id, out var existingEmail))
                {
                    dbContext.SupportEmails.Update(existingEmail);
                    emailsDict.Remove(existEmail.Id);
                }
                else
                {
                    dbContext.SupportEmails.Remove(existEmail);
                }
            }
            
            dbContext.SupportEmails.AddRange(emailsDict.Values);
            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Failed to create support email.");
        }
    }
}