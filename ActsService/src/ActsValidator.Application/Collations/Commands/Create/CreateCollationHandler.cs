using ActsValidator.Application.Abstractions;
using ActsValidator.Application.Providers;
using ActsValidator.Application.Repositories;
using ActsValidator.Domain;
using ActsValidator.Domain.Shared;
using ActsValidator.Domain.Shared.ValueObjects;
using ActsValidator.Domain.Shared.ValueObjects.Dtos;
using ActsValidator.Domain.Shared.ValueObjects.Ids;
using ActsValidator.Domain.ValueObjects;
using AiMessaging.Contracts.Messaging;
using CSharpFunctionalExtensions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ActsValidator.Application.Collations.Commands.Create;

public class CreateCollationHandler : ICommandHandler<CreateCollationCommand, Result<CollationDto, ErrorList>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppRepository _appRepository;
    private readonly IFileProvider _fileProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<CreateCollationHandler> _logger;

    public CreateCollationHandler(
        IUnitOfWork unitOfWork, 
        IAppRepository appRepository,
        IFileProvider fileProvider,
        IPublishEndpoint publishEndpoint,
        ILogger<CreateCollationHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _appRepository = appRepository;
        _fileProvider = fileProvider;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }

    public async Task<Result<CollationDto, ErrorList>> Handle(
        CreateCollationCommand command, 
        CancellationToken cancellationToken = default)
    {
        using var transaction = await _unitOfWork.BeginTransaction(cancellationToken);
        
        var file1Cells = _fileProvider.GetCollationRows(command.Stream1);

        if (file1Cells.IsFailure)
            return file1Cells.Error;
        
        var file2Cells = _fileProvider.GetCollationRows(command.Stream2, true);
        
        if (file2Cells.IsFailure)
            return file2Cells.Error;
        
        var collationResult = Collation
            .Create(command.UserId, command.Act1Name, command.Act2Name, file1Cells.Value, file2Cells.Value);
        
        if (collationResult.IsFailure)
            return collationResult.Error;

        try
        {
            var createCollationResult = _appRepository.AddCollation(collationResult.Value);

            var aiRequest = new AiRequest(AiRequestId.AddNewId(), CollationId.Create(createCollationResult));
            var createAiRequestResult = _appRepository.AddAiRequest(aiRequest);

            var prompt = GeneratePrompt(file1Cells.Value, file2Cells.Value);
            var sendToAiCommand = new SendToAiCommand(prompt, createAiRequestResult);
            
            await _publishEndpoint.Publish(sendToAiCommand, cancellationToken);
            
            await _unitOfWork.SaveChanges(cancellationToken);

            var result = GetResult(collationResult.Value);
            
            transaction.Commit();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError("Unable to create collation. {error}", ex.Message);
            transaction.Rollback();

            return (ErrorList)Error.Failure(
                "collation.create.failure", 
                $"Unable to create collation: {ex.Message}");
        }
    }

    private CollationDto GetResult(Collation collation)
    {
        var discrepancies = collation.Discrepancies
            .Select(x =>
            {
                var c1 = x.Act1 is not null 
                    ? new CollationRowDto(x.Act1.SerialNumber, x.Act1.Date, x.Act1.Debet, x.Act1.Credit) : null;
                
                var c2 = x.Act2 is not null 
                    ? new CollationRowDto(x.Act2.SerialNumber, x.Act2.Date, x.Act2.Debet, x.Act2.Credit) : null;
                
                return new DiscrepancyDto(c1, c2, x.CellName);
            })
            .ToArray();
        
        return new CollationDto(collation.UserId, collation.Id, collation.Act1Name, collation.Act2Name, discrepancies);
    }

    private string GeneratePrompt(IEnumerable<CollationRow> act1, IEnumerable<CollationRow> act2)
    {
        var act1ToJson = JsonConvert.SerializeObject(act1);
        var act2ToJson = JsonConvert.SerializeObject(act2);

        var collation1Example = CollationRow
            .Create(1, DateTime.UtcNow, (decimal)1451.65, 0).Value;
        
        var collation2Example = CollationRow
            .Create(8, DateTime.UtcNow, 0, (decimal)7457.5).Value;
        
        var collation3Example = CollationRow
            .Create(5, DateTime.UtcNow - TimeSpan.FromDays(5), 8749, 0).Value;
        
        var collation4Example = CollationRow
            .Create(10, DateTime.UtcNow - TimeSpan.FromDays(10), 0, 8749).Value;

        var discrepanciesExample = new List<Discrepancy>()
        {
            Discrepancy.Create(collation1Example, collation2Example, "дебет").Value,
            Discrepancy.Create(collation3Example, collation4Example, "время").Value
        };
        
        var discrepanciesExampleToJson = JsonConvert.SerializeObject(discrepanciesExample);
        
        var prompt = $"I have two reconciliation statements with debit, credit and date: " +
                     $"{act1ToJson} and {act2ToJson}. You are an accountant and " +
                     $"I need you to find the discrepancies and send them in JSON format. " +
                     $"For example: {discrepanciesExampleToJson}";
        
        return prompt;
    }
}