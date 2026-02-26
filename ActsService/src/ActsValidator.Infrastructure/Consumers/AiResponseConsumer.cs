using ActsValidator.Application.Abstractions;
using ActsValidator.Domain.Shared;
using ActsValidator.Domain.Shared.ValueObjects;
using ActsValidator.Domain.Shared.ValueObjects.Dtos;
using ActsValidator.Domain.ValueObjects;
using ActsValidator.Infrastructure.DbContexts;
using ActsValidator.Presentation.Extensions;
using AiMessaging.Contracts.Messaging;
using CSharpFunctionalExtensions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ActsValidator.Infrastructure.Consumers;

public class AiResponseConsumer : IConsumer<AiResponseEvent>
{
    private readonly AppDbContext _appDbContext;
    private readonly ILogger<AiResponseConsumer> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AiResponseConsumer(AppDbContext appDbContext, ILogger<AiResponseConsumer> logger, IUnitOfWork unitOfWork)
    {
        _appDbContext = appDbContext;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }
    
    public async Task Consume(ConsumeContext<AiResponseEvent> context)
    {
        var aiRequestExist = await _appDbContext.AiRequests
            .FirstOrDefaultAsync(r => r.Id == context.Message.AiRequestId);

        if (aiRequestExist is null)
        {
            _logger.LogError("Can't find AiRequest with Id {id}", context.Message.AiRequestId);
            return;
        }

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
                
                var act1 = d.Act1 is not null
                    ? CollationRow.Create(d.Act1.SerialNumber, d.Act1.Date, d.Act1.Debet, d.Act1.Credit)
                    : (Result<CollationRow, ErrorList>?)null;

                if (act1 is not null && act1.Value.IsFailure)
                    errors.AddRange(act1.Value.Error);

                var act2 = d.Act2 is not null
                    ? CollationRow.Create(d.Act2.SerialNumber, d.Act2.Date, d.Act2.Debet, d.Act2.Credit)
                    : (Result<CollationRow, ErrorList>?)null;

                if (act2 is not null && act2.Value.IsFailure)
                {
                    errors.AddRange(act2.Value.Error);
                    return Result.Failure<Discrepancy, ErrorList>(errors);
                }

                var discrepancy = Discrepancy.Create(act1?.Value, act2?.Value, d.CellName);
                
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
        
        aiRequestExist.Complete(discrepancies.Select(x => x.Value).ToList());
        
        await _unitOfWork.SaveChanges(context.CancellationToken);
    }
}