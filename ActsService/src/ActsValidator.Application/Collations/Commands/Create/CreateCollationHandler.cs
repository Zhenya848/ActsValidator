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
    private readonly IGreeterService _greeterService;

    public CreateCollationHandler(
        IUnitOfWork unitOfWork, 
        IAppRepository appRepository,
        IFileProvider fileProvider,
        IPublishEndpoint publishEndpoint,
        ILogger<CreateCollationHandler> logger,
        IGreeterService greeterService)
    {
        _unitOfWork = unitOfWork;
        _appRepository = appRepository;
        _fileProvider = fileProvider;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
        _greeterService = greeterService;
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
            _appRepository.AddCollation(collationResult.Value);

            var aiRequest = new AiRequest(AiRequestId.AddNewId(), collationResult.Value);
            var createAiRequestResult = _appRepository.AddAiRequest(aiRequest);

            var prompt = GeneratePrompt(file1Cells.Value, file2Cells.Value);
            var sendToAiCommand = new SendToAiCommand(createAiRequestResult, prompt);
            
            await _publishEndpoint.Publish(sendToAiCommand, cancellationToken);
            
            await _unitOfWork.SaveChanges(cancellationToken);

            var result = GetResult(collationResult.Value, aiRequest.Status);

            var makeActionResult = await _greeterService.MakeAction(command.UserId, cancellationToken);

            if (makeActionResult.IsFailure)
            {
                transaction.Rollback();
                return makeActionResult.Error;
            }
            
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

    private CollationDto GetResult(Collation collation, AiRequestStatus aiRequestStatus)
    {
        var discrepancies = collation.CollationErrors
            .Select(x => new DiscrepancyDto(
                x.Act1Row, x.Act2Row, x.Act1Value, x.Act2Value, x.Field, x.Difference, x.Severity, x.DetectedBy.ToArray()))
            .ToArray();
        
        return new CollationDto(
            collation.UserId, 
            collation.Id, 
            collation.Act1Name, 
            collation.Act2Name, 
            collation.CoincidencesCount, 
            collation.RowsProcessed, 
            discrepancies, 
            collation.Status.ToString(), 
            aiRequestStatus.ToString(), 
            collation.CreatedAt);
    }

    private string GeneratePrompt(IEnumerable<CollationRow> act1, IEnumerable<CollationRow> act2)
    {
        /*var act1ToJson = JsonConvert.SerializeObject(act1);
        var act2ToJson = JsonConvert.SerializeObject(act2);

        var act1Example = new List<CollationRow>()
        {
            CollationRow.Create(11, DateTime.UtcNow.AddMonths(-1), 1521, 0).Value,
            CollationRow.Create(12, DateTime.UtcNow.AddMonths(-4).AddDays(-3), (decimal)1976.4, 0).Value,
            CollationRow.Create(13, DateTime.UtcNow.AddMonths(-5).AddDays(-9), 1000, 0).Value,
            CollationRow.Create(14, DateTime.UtcNow.AddMonths(-4).AddDays(-2), (decimal)1100.6, 0).Value,
            CollationRow.Create(15, DateTime.UtcNow.AddMonths(-5).AddDays(-11), 0, 860).Value,
            CollationRow.Create(16, DateTime.UtcNow.AddMonths(-5).AddDays(-13), 0, 1500).Value
        };
        
        var act2Example = new List<CollationRow>()
        {
            CollationRow.Create(20, DateTime.UtcNow.AddMonths(-5).AddDays(-9), 0, (decimal)997.6).Value,
            CollationRow.Create(21, DateTime.UtcNow.AddMonths(-5).AddDays(-10), 0, 1000).Value,
            CollationRow.Create(22, DateTime.UtcNow.AddMonths(-2), 0, 1521).Value,
            CollationRow.Create(23, DateTime.UtcNow.AddMonths(-4).AddDays(-3), 0, (decimal)1976.4).Value,
            CollationRow.Create(24, DateTime.UtcNow.AddMonths(-4).AddDays(-2), 0, (decimal)6000.4).Value
        };

        var discrepanciesExample = new List<Discrepancy>()
        {
            Discrepancy.Create(act1Example[0], act2Example[2]).Value,
            Discrepancy.Create(act1Example[2], act2Example[0]).Value,
            Discrepancy.Create(null, act2Example[1]).Value,
            Discrepancy.Create(act1Example[3], act2Example[4]).Value,
            Discrepancy.Create(act1Example[4], null).Value,
            Discrepancy.Create(act1Example[5], null).Value
        };
        
        discrepanciesExample.ForEach(d => d.AddDetectedCharacter(Constants.DetectedBy.Ai));
        
        var act1ExampleToJson = JsonConvert.SerializeObject(act1Example);
        var act2ExampleToJson = JsonConvert.SerializeObject(act2Example);
        var discrepanciesExampleToJson = JsonConvert.SerializeObject(discrepanciesExample);
        
        var prompt = $"I have two reconciliation statements with debit, credit and date: " +
                     $"{act1ToJson} and {act2ToJson}. You are an accountant and " +
                     $"I need you to find the discrepancies and send them in JSON format. " +
                     $"For example: act1 - {act1ExampleToJson} and act2 - {act2ExampleToJson}. " +
                     $"Discrepancies - {discrepanciesExampleToJson}";
        
        return prompt;*/
        
        return "";
    }
}