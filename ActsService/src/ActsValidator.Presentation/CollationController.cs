using ActsValidator.Application.Collations.Create;
using ActsValidator.Presentation.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ActsValidator.Presentation;

[ApiController]
[Route("api/[controller]")]
public class CollationController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromForm] IFormFileCollection files,
        [FromServices] CreateCollationHandler handler,
        CancellationToken cancellationToken = default)
    {
        await using var stream1 = files[0].OpenReadStream();
        await using var stream2 = files[1].OpenReadStream();
        
        var command = new CreateCollationCommand(stream1, stream2);
        
        var result = await handler.Handle(command, cancellationToken);

        if (result.IsFailure)
            return result.Error.ToResponse();
        
        return Ok(Envelope.Ok(result.Value));
    }
}