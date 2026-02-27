using ActsValidator.Application.Abstractions;
using ActsValidator.Application.Repositories;
using ActsValidator.Domain.Shared.ValueObjects.Dtos;

namespace ActsValidator.Application.Collations.Queries;

public class GetCollationsWithPaginationHandler : IQueryHandler<GetCollationsWithPaginationCommand, PagedList<CollationDto>>
{
    private readonly IAppRepository _appRepository;
    
    public GetCollationsWithPaginationHandler(IAppRepository appRepository)
    {
        _appRepository = appRepository;
    }
    
    public Task<PagedList<CollationDto>> Handle(GetCollationsWithPaginationCommand query, CancellationToken cancellationToken = default)
    {
        
    }
}