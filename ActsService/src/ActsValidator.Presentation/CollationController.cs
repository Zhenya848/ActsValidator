using ActsValidator.Application.Collations.Commands.Create;
using ActsValidator.Application.Collations.Queries;
using ActsValidator.Domain.Shared;
using ActsValidator.Presentation.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ActsValidator.Presentation;

[ApiController]
[Route("api/[controller]")]
public class CollationController : ControllerBase
{
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromForm] IFormFileCollection files,
        [FromServices] CreateCollationHandler handler,
        CancellationToken cancellationToken = default)
    {
        if (files.Count < 2)
        {
            var error = Error.NotFound("files.not.fount", "Files count must be greater than 2");
            return BadRequest(Envelope.Error(error));
        }

        var userId = User.GetUserIdRequired();
        
        await using var stream1 = files[0].OpenReadStream();
        await using var stream2 = files[1].OpenReadStream();

        var act1Name = Path.GetFileName(files[0].FileName);
        var act2Name = Path.GetFileName(files[1].FileName);
        
        var command = new CreateCollationCommand(userId, act1Name, act2Name, stream1, stream2);
        
        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();
        
        return Ok(Envelope.Ok(result.Value));
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Get(
        [FromRoute] Guid userId,
        [FromServices] GetCollationsWithPaginationHandler withPaginationHandler,
        CancellationToken cancellationToken = default)
    {
        
    }
}