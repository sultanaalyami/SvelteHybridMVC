using HRCE.Services.BuildOperations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRCE.Controllers;

[ApiController]
[Authorize(Policy = "AdminOnly")]
[Route("api/buildops")]
public sealed class BuildOpsApiController : ControllerBase
{
    private readonly BuildOperationsService _operations;

    public BuildOpsApiController(BuildOperationsService operations)
    {
        _operations = operations;
    }

    [HttpGet("commands")]
    public IActionResult GetCommands()
    {
        var commands = _operations.GetCommands().Select(command => new
        {
            key = command.Key,
            label = command.Label,
            description = command.Description
        });

        return Ok(commands);
    }

    [HttpGet("jobs")]
    public IActionResult GetJobs()
    {
        return Ok(_operations.GetJobs());
    }

    [HttpGet("jobs/{id:guid}")]
    public IActionResult GetJob(Guid id)
    {
        var job = _operations.GetJob(id);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpPost("run")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Run([FromBody] BuildRunRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CommandKey))
        {
            return BadRequest(new { error = "Command key is required." });
        }

        var job = await _operations.StartAsync(request.CommandKey);
        return job is null ? NotFound(new { error = "Command not found." }) : Ok(job);
    }

    [HttpPost("cancel/{id:guid}")]
    [ValidateAntiForgeryToken]
    public IActionResult Cancel(Guid id)
    {
        var canceled = _operations.Cancel(id);
        return canceled ? Ok() : NotFound();
    }
}

public sealed record BuildRunRequest(string CommandKey);
