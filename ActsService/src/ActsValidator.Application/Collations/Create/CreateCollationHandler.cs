using ActsValidator.Application.Abstractions;
using ActsValidator.Application.Providers;
using ActsValidator.Application.Repositories;
using ActsValidator.Domain;
using ActsValidator.Domain.Shared;
using CSharpFunctionalExtensions;

namespace ActsValidator.Application.Collations.Create;

public class CreateCollationHandler : ICommandHandler<CreateCollationCommand, Result<Guid, ErrorList>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICollationRepository _collationRepository;
    private readonly IFileProvider _fileProvider;

    public CreateCollationHandler(
        IUnitOfWork unitOfWork, 
        ICollationRepository collationRepository,
        IFileProvider fileProvider)
    {
        _unitOfWork = unitOfWork;
        _collationRepository = collationRepository;
        _fileProvider = fileProvider;
    }

    public async Task<Result<Guid, ErrorList>> Handle(
        CreateCollationCommand command, 
        CancellationToken cancellationToken = default)
    {
        var file1Cells = _fileProvider.GetCollationRows(command.File1);

        if (file1Cells.IsFailure)
            return file1Cells.Error;
        
        var file2Cells = _fileProvider.GetCollationRows(command.File2, true);
        
        if (file2Cells.IsFailure)
            return file2Cells.Error;
        
        var collationResult = Collation.Create(file1Cells.Value, file2Cells.Value);
        
        if (collationResult.IsFailure)
            return collationResult.Error;

        var createResult = _collationRepository.Add(collationResult.Value);
        await _unitOfWork.SaveChanges(cancellationToken);
        
        return createResult;
    }
}