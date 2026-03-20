using Core;
using Framework;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Models.Shared;

namespace PaymentService.Extensions;

public static class ApiExtensions
{
    public static IResult ToIResultResponse(this Error error)
    {
        var envelope = Envelope.Error(error);
        
        return Results.Json(
            data: envelope,
            statusCode: GetStatusCode(error),
            contentType: "application/json"
        );
    }

    public static IResult ToIResultResponse(this ErrorList errors)
    {
        if (errors.Any() == false)
            return Results.BadRequest();
        
        Envelope envelope = Envelope.Error(errors);
        
        return Results.Json(
            data: envelope,
            statusCode: GetStatusCode(errors),
            contentType: "application/json"
        );
    }

    private static int GetStatusCode(ErrorList errors)
    {
        var statusCode = errors.Count() > 1 
            ? StatusCodes.Status500InternalServerError
            : errors.First().ErrorType switch
            {
                ErrorType.Validation => StatusCodes.Status400BadRequest,
                ErrorType.Required => StatusCodes.Status400BadRequest,
                ErrorType.NotFound => StatusCodes.Status404NotFound,
                ErrorType.Conflict => StatusCodes.Status409Conflict,
                ErrorType.Failure => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };
        
        return statusCode;
    }
}