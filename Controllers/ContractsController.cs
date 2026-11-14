using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMove.Models;
using TechMove.Services.ApiClients;

namespace TechMove.Controllers;

[Authorize]
public sealed class ContractsController(IContractApiService contracts, IClientApiService clients) : Controller
{
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, ContractStatus? status, CancellationToken cancellationToken)
    {
        var result = await contracts.SearchAsync(startDate, endDate, status, cancellationToken);
        if (!result.IsSuccess) TempData["Error"] = result.ErrorMessage;
        return View(result.Value ?? Array.Empty<Contract>());
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var result = await contracts.GetAsync(id, cancellationToken);
        if (!result.IsSuccess) return NotFound();
        return View(result.Value);
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await PopulateClientsAsync(cancellationToken);
        return View(new Contract());
    }

    [HttpPost, ValidateAntiForgeryToken, Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Contract contract, IFormFile? pdfFile, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            await PopulateClientsAsync(cancellationToken);
            return View(contract);
        }
        var result = await contracts.CreateAsync(contract, cancellationToken);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The contract could not be created.");
            await PopulateClientsAsync(cancellationToken);
            return View(contract);
        }
        if (pdfFile is not null)
        {
            var upload = await contracts.UploadAgreementAsync(result.Value!.Id, pdfFile, cancellationToken);
            if (!upload.IsSuccess)
            {
                ModelState.AddModelError(string.Empty, upload.ErrorMessage ?? "The agreement could not be uploaded.");
                return View(contract);
            }
        }
        TempData["Success"] = "Contract created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Upload(int id, IFormFile file, CancellationToken cancellationToken)
    {
        var result = await contracts.UploadAgreementAsync(id, file, cancellationToken);
        TempData[result.IsSuccess ? "Success" : "Error"] = result.IsSuccess ? "Agreement uploaded." : result.ErrorMessage;
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var result = await contracts.DownloadAgreementAsync(id, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            TempData["Error"] = result.ErrorMessage;
            return RedirectToAction(nameof(Details), new { id });
        }
        return File(result.Value.Stream, "application/pdf", result.Value.FileName);
    }

    private async Task PopulateClientsAsync(CancellationToken cancellationToken)
    {
        var result = await clients.GetAllAsync(cancellationToken);
        ViewData["ClientId"] = new SelectList(result.Value ?? Array.Empty<Client>(), "Id", "Name");
        if (!result.IsSuccess) ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Clients could not be loaded.");
    }
}
