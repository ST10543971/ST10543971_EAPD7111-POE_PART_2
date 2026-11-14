using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMove.Api.Dtos;
using TechMove.Api.Models;
using TechMove.Api.Services;

namespace TechMove.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ContractsController(IContractService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ContractDto>>> Search([FromQuery] DateTime? startFrom, [FromQuery] DateTime? endTo, [FromQuery] ContractStatus? status, CancellationToken cancellationToken)
        => Ok((await service.SearchAsync(startFrom, endTo, status, cancellationToken)).Select(x => x.ToDto()));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ContractDto>> Get(int id, CancellationToken cancellationToken)
        => Ok((await service.GetAsync(id, cancellationToken)).ToDto());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ContractDto>> Create(CreateContractDto dto, CancellationToken cancellationToken)
    {
        var contract = await service.CreateAsync(new Contract { ClientId = dto.ClientId, StartDate = dto.StartDate, EndDate = dto.EndDate, Status = dto.Status, ServiceLevel = dto.ServiceLevel }, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = contract.Id }, contract.ToDto());
    }

    [HttpPatch("{id:int}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ContractDto>> UpdateStatus(int id, UpdateContractStatusDto dto, CancellationToken cancellationToken)
        => Ok((await service.UpdateStatusAsync(id, dto.Status, cancellationToken)).ToDto());

    [HttpPost("{id:int}/agreement")]
    [Authorize(Roles = "Admin")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult> UploadAgreement(int id, IFormFile file, CancellationToken cancellationToken)
    {
        await service.SaveAgreementAsync(id, file, cancellationToken);
        return Ok();
    }

    [HttpGet("{id:int}/agreement")]
    public async Task<IActionResult> DownloadAgreement(int id, CancellationToken cancellationToken)
    {
        var agreement = await service.OpenAgreementAsync(id, cancellationToken);
        return agreement is null ? NotFound() : File(agreement.Value.Stream, "application/pdf", agreement.Value.FileName);
    }
}
