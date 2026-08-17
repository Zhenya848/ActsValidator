using Quartz;

namespace ChatService.Workers.Email;

public class ProcessEmailDeliveriesJob : IJob
{
    private readonly ProcessEmailDeliveriesService _processEmailDeliveriesService;

    public ProcessEmailDeliveriesJob(ProcessEmailDeliveriesService processEmailDeliveriesService)
    {
        _processEmailDeliveriesService = processEmailDeliveriesService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        await _processEmailDeliveriesService.Execute(context.CancellationToken);
    }
}