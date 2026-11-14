using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMove.Api.Dtos;
using TechMove.Api.Models;
using TechMove.Api.Services;

namespace TechMove.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ServiceRequestsController(IServiceRequestService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceRequestDto>>> Get([FromQuery] int? contractId, CancellationToken cancellationToken)
        => Ok((await service.GetAsync(contractId, cancellationToken)).Select(x => x.ToDto()));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ServiceRequestDto>> GetById(int id, CancellationToken cancellationToken)
        => Ok((await service.GetAsync(id, cancellationToken)).ToDto());

    [HttpPost]
    public async Task<ActionResult<ServiceRequestDto>> Create(CreateServiceRequestDto dto, CancellationToken cancellationToken)
    {
        var request = await service.CreateAsync(new ServiceRequest { ContractId = dto.ContractId, Description = dto.Description, UsdAmount = dto.AmountUsd, Status = dto.Status }, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = request.Id }, request.ToDto());
    }
}
