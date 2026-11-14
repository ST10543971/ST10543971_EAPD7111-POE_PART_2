using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMove.Models;
using TechMove.Services.ApiClients;

namespace TechMove.Controllers;

[Authorize]
public sealed class ServiceRequestsController(IServiceRequestApiService requests, IContractApiService contracts, IExchangeRateApiService rates) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await requests.GetAsync(cancellationToken: cancellationToken);
        if (!result.IsSuccess) TempData["Error"] = result.ErrorMessage;
        return View(result.Value ?? Array.Empty<ServiceRequest>());
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var result = await requests.GetAsync(id, cancellationToken: cancellationToken);
        return !result.IsSuccess || result.Value is null ? NotFound() : View(result.Value);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateContractsAsync(cancellationToken);
        return View(new ServiceRequest());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateContractsAsync(cancellationToken);
            return View(request);
        }
        var result = await requests.CreateAsync(request, cancellationToken);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The service request could not be created.");
            await PopulateContractsAsync(cancellationToken);
            return View(request);
        }
        TempData["Success"] = "Service request created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetRate(CancellationToken cancellationToken)
    {
        var result = await rates.GetUsdZarAsync(cancellationToken);
        return result.IsSuccess ? Json(result.Value) : StatusCode(result.StatusCode == 0 ? 503 : result.StatusCode, new { error = result.ErrorMessage });
    }

    private async Task PopulateContractsAsync(CancellationToken cancellationToken)
    {
        var result = await contracts.SearchAsync(null, null, null, cancellationToken);
        ViewData["ContractId"] = new SelectList(result.Value ?? Array.Empty<Contract>(), "Id", "ServiceLevel");
        if (!result.IsSuccess) ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Contracts could not be loaded.");
    }
}
