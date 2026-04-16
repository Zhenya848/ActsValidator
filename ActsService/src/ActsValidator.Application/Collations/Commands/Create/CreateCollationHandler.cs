using ActsValidator.Application.Abstractions;
using ActsValidator.Application.Providers;
using ActsValidator.Application.Repositories;
using ActsValidator.Domain;
using ActsValidator.Domain.Shared;
using ActsValidator.Domain.Shared.ValueObjects.Dtos;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;

namespace ActsValidator.Application.Collations.Commands.Create;

public class CreateCollationHandler : ICommandHandler<CreateCollationCommand, Result<CollationDto, ErrorList>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAppRepository _appRepository;
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<CreateCollationHandler> _logger;
    private readonly IGreeterService _greeterService;

    public CreateCollationHandler(
        IUnitOfWork unitOfWork, 
        IAppRepository appRepository,
        IFileProvider fileProvider,
        ILogger<CreateCollationHandler> logger,
        IGreeterService greeterService)
    {
        _unitOfWork = unitOfWork;
        _appRepository = appRepository;
        _fileProvider = fileProvider;
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
            
            await _unitOfWork.SaveChanges(cancellationToken);

            var makeActionResult = await _greeterService.MakeAction(command.UserId, cancellationToken);

            if (makeActionResult.IsFailure)
            {
                transaction.Rollback();
                return makeActionResult.Error;
            }
            
            transaction.Commit();

            return GetResult(collationResult.Value);
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
        var discrepancies = collation.CollationErrors
            .Select(x => new DiscrepancyDto(
                x.Act1Row, x.Act2Row, x.Act1Value, x.Act2Value, x.Field, x.Difference, x.Severity))
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
            collation.CreatedAt);
    }
}