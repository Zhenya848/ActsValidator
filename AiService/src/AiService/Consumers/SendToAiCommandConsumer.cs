using AiMessaging.Contracts.Messaging;
using AiService.Providers;
using MassTransit;
using MassTransit.Transports;

namespace AiService.Consumers;

public class SendToAiCommandConsumer : IConsumer<SendToAiCommand>
{
    private readonly AiProvider _aiProvider;
    private readonly IPublishEndpoint _publishEndpoint;

    public SendToAiCommandConsumer(AiProvider aiProvider, IPublishEndpoint publishEndpoint)
    {
        _aiProvider = aiProvider;
        _publishEndpoint = publishEndpoint;
    }
    
    public async Task Consume(ConsumeContext<SendToAiCommand> context)
    {
        var aiResponse = await _aiProvider
            .SendToAi(context.Message.Prompt, null, context.CancellationToken);

        if (aiResponse.IsFailure)
        {
            var error = $"{aiResponse.Error.Code}: {aiResponse.Error.Message}";
            var eventError = new AiResponseEvent(context.Message.AiRequestId, null, error);
            
            await _publishEndpoint.Publish(eventError);
            
            return;
        }
        
        var eventResult = new AiResponseEvent(context.Message.AiRequestId, aiResponse.Value);
        
        await _publishEndpoint.Publish(eventResult);
    }
}