using ChatService.Providers;
using Quartz;

namespace ChatService.Workers.Outbox;

[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob : IJob
{
    private readonly ProcessOutboxMessagesService _outboxMessagesService;
    private readonly SupportEmailsProvider _supportEmailsProvider;

    public ProcessOutboxMessagesJob(
        ProcessOutboxMessagesService outboxMessagesService, 
        SupportEmailsProvider supportEmailsProvider)
    {
        _outboxMessagesService = outboxMessagesService;
        _supportEmailsProvider = supportEmailsProvider;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        await _supportEmailsProvider.InitializeAsync(context.CancellationToken);
        await _outboxMessagesService.Execute(context.CancellationToken);
    }
}