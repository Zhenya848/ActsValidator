using ActsValidator.Application.Abstractions;
using ActsValidator.Domain.Shared;
using ActsValidator.Domain.Shared.ValueObjects;
using ActsValidator.Domain.Shared.ValueObjects.Dtos;
using ActsValidator.Domain.ValueObjects;
using ActsValidator.Infrastructure.DbContexts;
using ActsValidator.Infrastructure.Hubs;
using ActsValidator.Presentation.Extensions;
using AiMessaging.Contracts.Messaging;
using CSharpFunctionalExtensions;
using MassTransit;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ActsValidator.Infrastructure.Consumers;

public class AiResponseConsumer : IConsumer<AiResponseEvent>
{
    private readonly AppDbContext _appDbContext;
    private readonly ILogger<AiResponseConsumer> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHubContext<AnalysisHub> _hubContext;

    public AiResponseConsumer(
        AppDbContext appDbContext, 
        ILogger<AiResponseConsumer> logger, 
        IUnitOfWork unitOfWork, 
        IHubContext<AnalysisHub> hubContext)
    {
        _appDbContext = appDbContext;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _hubContext = hubContext;
    }
    
    public async Task Consume(ConsumeContext<AiResponseEvent> context)
    {
        var aiRequestExist = await _appDbContext.AiRequests
            .Include(c => c.Collation)
            .FirstOrDefaultAsync(r => r.Id == context.Message.AiRequestId);

        if (aiRequestExist is null)
        {
            _logger.LogError("Can't find AiRequest with Id {id}", context.Message.AiRequestId);
            return;
        }
        
        if (context.Message.Error is not null)
        {
            _logger.LogError("Error - {error}", context.Message.Error);
            var setMessageResult = aiRequestExist.SetErrorMessage(context.Message.Error);
            
            if (setMessageResult.IsFailure)
                _logger.LogError("{code} - {message}", setMessageResult.Error.Code, setMessageResult.Error.Message);
            
            await _unitOfWork.SaveChanges(context.CancellationToken);
            return;
        }
        
        if (context.Message.Response is null)
            return;

        var discrepanciesDtoResult = context.Message.Response
            .ConvertJsonToType<List<DiscrepancyDto>>();

        if (discrepanciesDtoResult.IsFailure)
        {
            _logger.LogError(
                "{code} - {message}", 
                discrepanciesDtoResult.Error.Code, 
                discrepanciesDtoResult.Error.Message);
            
            return;
        }

        var discrepancies = discrepanciesDtoResult.Value
            .Select(d =>
            {
                var errors = new List<Error>();

                var discrepancy = Discrepancy
                    .Create(d.Act1Row, d.Act2Row, d.Act1Value, d.Act2Value, d.Field, d.Difference, d.Severity);
                
                if (discrepancy.IsFailure)
                    errors.AddRange(discrepancy.Error);
                
                return errors.Any() 
                    ? Result.Failure<Discrepancy, ErrorList>(errors) 
                    : Result.Success<Discrepancy, ErrorList>(discrepancy.Value);
            })
            .ToList();
        

        if (discrepancies.Any(x => x.IsFailure))
        {
            var error = string.Join(", ", discrepancies.SelectMany(x => 
                x.Error.Select(j => $"{j.Code} - {j.Message}, ")));

            _logger.LogError("Failure to create discrepancies: {error}", error);
            
            return;
        }
        
        discrepancies.ForEach(x =>
        {
            if (aiRequestExist.Collation.CollationErrors.TryGetValue(x.Value, out var discrepancy))
                discrepancy.AddDetectedCharacter(Constants.DetectedBy.Ai);
            else
                aiRequestExist.Collation.CollationErrors.Add(x.Value);
        });

        aiRequestExist.Complete();
        
        await _unitOfWork.SaveChanges(context.CancellationToken);

        var discrepanciesForHub = aiRequestExist.Collation.CollationErrors
            .Select(x => new DiscrepancyDto(x.Act1Row, x.Act2Row, x.Act1Value, x.Act2Value,
                x.Field, x.Difference, x.Severity, x.DetectedBy.ToArray()));
        
        await _hubContext.Clients.User(aiRequestExist.Collation.UserId.ToString())
            .SendAsync("ReceiveAiAnalysis", new { 
                requestId = context.Message.AiRequestId, 
                discrepancies = discrepanciesForHub
            });
    }
}