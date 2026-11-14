using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMove.Api.Dtos;
using TechMove.Api.Models;
using TechMove.Api.Services;

namespace TechMove.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ClientsController(IClientService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClientDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll(CancellationToken cancellationToken)
        => Ok((await service.GetAllAsync(cancellationToken)).Select(x => x.ToDto()));

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClientDto>> Get(int id, CancellationToken cancellationToken)
        => Ok((await service.GetAsync(id, cancellationToken)).ToDto());

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ClientDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<ClientDto>> Create(CreateClientDto dto, CancellationToken cancellationToken)
    {
        var client = await service.CreateAsync(new Client { Name = dto.Name, ContactDetails = dto.ContactDetails, Region = dto.Region }, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = client.Id }, client.ToDto());
    }
}
