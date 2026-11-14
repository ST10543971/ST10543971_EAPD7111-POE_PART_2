using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMove.Api.Dtos;
using TechMove.Api.Services;

namespace TechMove.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ExchangeRatesController(ICurrencyService currencyService) : ControllerBase
{
    [HttpGet("usd-zar")]
    [ProducesResponseType(typeof(ExchangeRateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<ExchangeRateDto>> GetUsdZar(CancellationToken cancellationToken)
    {
        try
        {
            var rate = await currencyService.GetUsdToZarRateAsync(cancellationToken);
            return Ok(new ExchangeRateDto(rate, DateTime.UtcNow));
        }
        catch (HttpRequestException)
        {
            return Problem("The exchange-rate service is temporarily unavailable.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }
        catch (TaskCanceledException)
        {
            return Problem("The exchange-rate service timed out.", statusCode: StatusCodes.Status503ServiceUnavailable);
        }
    }
}
