using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMove.Models;
using TechMove.Services.ApiClients;

namespace TechMove.Controllers;

[Authorize]
public sealed class ClientsController(IClientApiService clients) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var result = await clients.GetAllAsync(cancellationToken);
        if (!result.IsSuccess) TempData["Error"] = result.ErrorMessage;
        return View(result.Value ?? Array.Empty<Client>());
    }

    public IActionResult Create() => View(new Client());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Client client, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(client);
        var result = await clients.CreateAsync(client, cancellationToken);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The client could not be created.");
            return View(client);
        }
        TempData["Success"] = "Client created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var result = await clients.GetAllAsync(cancellationToken);
        var client = result.Value?.FirstOrDefault(x => x.Id == id);
        return client is null ? NotFound() : View(client);
    }
}
