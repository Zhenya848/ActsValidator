using System.Text;
using System.Text.Json;
using ActsValidator.Application.Abstractions;
using ActsValidator.Application.Extensions;
using ActsValidator.Application.Repositories;
using ActsValidator.Domain.Shared.ValueObjects.Dtos;
using CSharpFunctionalExtensions;
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

            var sql = $@"
                SELECT 
                    c.user_id as UserId, 
                    c.id as Id, 
                    c.act1name as Act1Name, 
                    c.act2name as Act2Name, 
                    c.discrepancies as Discrepancies,
                    ar.id as Id,
                    ar.status as Status,
                    ar.error_message as ErrorMessage,
                    ar.discrepancies as Discrepancies
                FROM collations c
                LEFT JOIN ai_requests ar ON ar.collation_id = c.id
                WHERE c.user_id = @UserId
                AND (@ActName IS NULL OR c.act1name ILIKE '%' || @ActName || '%' OR c.act2name ILIKE '%' || @ActName || '%')
                ORDER BY c.{query.OrderBy ?? "id"} {(query.OrderByDesc ? "DESC" : "ASC")}
                LIMIT @PageSize OFFSET @Offset";

            var items = await connection.QueryAsync<CollationDto, AiRequestDto?, CollationDto>(
                sql,
                (collation, aiRequest) => 
                {
                    return collation with { AiRequest = aiRequest };
                },
                new { 
                    query.UserId, 
                    query.ActName,
                    query.PageSize, 
                    Offset = (query.Page - 1) * query.PageSize 
                },
                splitOn: "Id"
            );
            
            var countSql = "SELECT COUNT(*) FROM collations WHERE user_id = @UserId";
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql, new { query.UserId });
            
            return new PagedList<CollationDto>()
            {
                Items = items.ToList(),
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount
            };
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error in GetCollationsWithPaginationHandler");
            return new PagedList<CollationDto>();
        }
    }
}