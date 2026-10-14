using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechMove.Data;
using TechMove.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace TechMove.Controllers
{
    public class ContractsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ContractsController(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }
        // GET: Contracts
        public async Task<IActionResult> Index(
      DateTime? startDate,
      DateTime? endDate,
      ContractStatus? status)
        {
            var contracts = _context.Contracts
                .Include(c => c.Client)
                .AsQueryable();

            if (startDate.HasValue)
            {
                contracts = contracts.Where(c =>
                    c.StartDate.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                contracts = contracts.Where(c =>
                    c.EndDate.Date <= endDate.Value.Date);
            }

            if (status.HasValue)
            {
                contracts = contracts.Where(c =>
                    c.Status == status.Value);
            }

            return View(await contracts.ToListAsync());
        }

        // GET: Contracts/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // GET: Contracts/Create
        public IActionResult Create()
        {
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Name");

            return View();
        }

        // POST: Contracts/Create

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contract contract, IFormFile? pdfFile)
        {
            
                if (ModelState.IsValid)
                {
                    if (pdfFile != null)
                    {
                        if (Path.GetExtension(pdfFile.FileName).ToLower() != ".pdf")
                        {
                            ModelState.AddModelError("", "Only PDF file is allowed.");
                            return View(contract);
                        }

                        string fileName =
                            Guid.NewGuid().ToString() +
                            Path.GetExtension(pdfFile.FileName);

                        string path = Path.Combine(
                            _environment.WebRootPath,
                            "agreements",
                            fileName);

                        using (var stream =
                               new FileStream(path, FileMode.Create))
                        {
                            await pdfFile.CopyToAsync(stream);
                        }

                        contract.SignedAgreementPath = fileName;
                    }

                    _context.Add(contract);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Name");
                return View(contract);
            }
        

        // GET: Contracts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts.FindAsync(id);
            if (contract == null)
            {
                return NotFound();
            }
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "Name");
            return View(contract);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
     
        public async Task<IActionResult> Edit(
    int id,
    [Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel,SignedAgreementPath")]
    Contract contract)
        {
            if (id != contract.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(contract);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContractExists(contract.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ClientId"] =
                new SelectList(_context.Clients,
                "Id",
                "Name",
                contract.ClientId);

            return View(contract);
        }

        // GET: Contracts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var contract = await _context.Contracts
                .Include(c => c.Client)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        // POST: Contracts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contract = await _context.Contracts.FindAsync(id);
            if (contract != null)
            {
                _context.Contracts.Remove(contract);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContractExists(int id)
        {
            return _context.Contracts.Any(e => e.Id == id);
        }
        public IActionResult Download(string fileName)
        {
            var path = Path.Combine(
                _environment.WebRootPath,
                "agreements",
                fileName);

            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            return PhysicalFile(
                path,
                "application/pdf",
                fileName);
        }
    }
}
