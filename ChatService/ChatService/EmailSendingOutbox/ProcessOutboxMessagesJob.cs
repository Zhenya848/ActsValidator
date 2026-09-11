using Quartz;

namespace ChatService.EmailSendingOutbox;

[DisallowConcurrentExecution]
public class ProcessOutboxMessagesJob(ProcessOutboxMessagesService outboxMessagesService) : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        await outboxMessagesService.Execute(context.CancellationToken);
    }
}