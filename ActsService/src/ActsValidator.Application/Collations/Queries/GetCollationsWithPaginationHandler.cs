using ActsValidator.Application.Abstractions;
using ActsValidator.Domain.Shared.ValueObjects.Dtos;
using Dapper;
using Microsoft.Extensions.Logging;

namespace ActsValidator.Application.Collations.Queries;

public class GetCollationsWithPaginationHandler : IQueryHandler<GetCollationsWithPaginationQuery, PagedList<CollationDto>>
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<GetCollationsWithPaginationHandler> _logger;
    
    public GetCollationsWithPaginationHandler(
        ISqlConnectionFactory connectionFactory, 
        ILogger<GetCollationsWithPaginationHandler> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }
    
    public async Task<PagedList<CollationDto>> Handle(
        GetCollationsWithPaginationQuery query, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _connectionFactory.Create();
            
            var isStatusFilter = query.StatusFilter is not null && query.StatusFilter.ToLower() != "all";
            var statusFilter = isStatusFilter ? query.StatusFilter!.ToLower() : string.Empty;
            var statusFilterSql = isStatusFilter ? " AND LOWER(c.status) = @statusFilter" : string.Empty;
            
            var sql = $@"
                SELECT 
                    c.user_id as UserId, 
                    c.id as Id, 
                    c.act1name as Act1Name, 
                    c.act2name as Act2Name, 
                    c.coincidences_count as CoincidencesCount,
                    c.rows_processed as RowsProcessed,
                    c.collation_errors as CollationErrors,
                    c.status as Status,
                    c.created_at as CreatedAt
                FROM collations c
                WHERE c.user_id = @UserId{statusFilterSql}
                AND (@ActName IS NULL OR c.act1name ILIKE '%' || @ActName || '%' OR c.act2name ILIKE '%' || @ActName || '%')
                ORDER BY c.created_at
                LIMIT @PageSize OFFSET @Offset";

            var items = await connection.QueryAsync<CollationDto>(
                sql,
                isStatusFilter  ?
                new { 
                    query.UserId, 
                    statusFilter,
                    query.ActName,
                    query.PageSize, 
                    Offset = (query.Page - 1) * query.PageSize 
                }
                : new { 
                    query.UserId,
                    query.ActName,
                    query.PageSize, 
                    Offset = (query.Page - 1) * query.PageSize 
                }
            );
            
            var countSql = $"SELECT COUNT(*) FROM collations c WHERE user_id = @UserId{statusFilterSql}";
            var totalCount = await connection.ExecuteScalarAsync<int>(
                countSql, 
                isStatusFilter ? new { query.UserId, statusFilter } : new { query.UserId });

            var averageAccuracySql = $"SELECT ROUND(a.coincidences_count * 2.0 / NULLIF(a.rows_processed, 0) * 100, 1) " +
                                     $"FROM collations a WHERE user_id = @UserId " +
                                     $"AND a.rows_processed - a.coincidences_count * 2 > 0";
            var averageAccuracy = (await connection.QueryAsync<float>(
                averageAccuracySql,
                new { query.UserId }
            )).ToArray();
            
            return new PagedList<CollationDto>()
            {
                Items = items.ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                SuccessfulCollations = totalCount - averageAccuracy.Length,
                AverageAccuracy = averageAccuracy.Sum()
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return new PagedList<CollationDto>();
        }
    }
}