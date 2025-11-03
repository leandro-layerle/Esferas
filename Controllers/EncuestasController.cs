using Esferas.Data;
using Esferas.Models.Entities;
using Esferas.Models.Enums;
using Esferas.Models.ViewModels;
using Esferas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Esferas.Controllers
{
    [Authorize]
    public class EncuestasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly LinkUnicoService _linkUnicoService;

        public EncuestasController(ApplicationDbContext context, LinkUnicoService linkUnicoService)
        {
            _context = context;
            _linkUnicoService = linkUnicoService;
        }

        public async Task<IActionResult> Index()
        {
            var encuestas = await _context.Encuestas
                .Include(e => e.Empresa)
                .ToListAsync();

            return View(encuestas);
        }

        public IActionResult Create()
        {
            var model = new EncuestaViewModel
            {
                EmpresasDisponibles = new SelectList(_context.Empresas.ToList(), "Id", "Nombre")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EncuestaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.EmpresasDisponibles = new SelectList(_context.Empresas.ToList(), "Id", "Nombre");
                return View(model);
            }

            var encuesta = new Encuesta
            {
                EmpresaId = model.EmpresaId,
                Estado = model.Estado,
                FechaCreacion = model.FechaCreacion
            };

            _context.Encuestas.Add(encuesta);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var encuesta = await _context.Encuestas.FindAsync(id);
            if (encuesta == null) return NotFound();

            var model = new EncuestaViewModel
            {
                Id = encuesta.Id,
                EmpresaId = encuesta.EmpresaId,
                Estado = encuesta.Estado,
                FechaCreacion = encuesta.FechaCreacion,
                EmpresasDisponibles = new SelectList(_context.Empresas.ToList(), "Id", "Nombre", encuesta.EmpresaId)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EncuestaViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                model.EmpresasDisponibles = new SelectList(_context.Empresas.ToList(), "Id", "Nombre", model.EmpresaId);
                return View(model);
            }

            var encuesta = await _context.Encuestas.FindAsync(id);
            if (encuesta == null) return NotFound();

            encuesta.EmpresaId = model.EmpresaId;
            encuesta.Estado = model.Estado;

            _context.Update(encuesta);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var encuesta = await _context.Encuestas
                .Include(e => e.Empresa)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (encuesta == null) return NotFound();

            return View(encuesta);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var encuesta = await _context.Encuestas.FindAsync(id);
            _context.Encuestas.Remove(encuesta);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarLinkEmpresa(int encuestaId)
        {
            var encuesta = await _context.Encuestas.FindAsync(encuestaId);
            if (encuesta == null)
                return NotFound();

            var url = await _linkUnicoService.GenerarLinkEmpresaAsync(encuestaId);

            TempData[$"LinkEmpresa_{encuestaId}"] = url;
            return RedirectToAction("Edit", new { id = encuestaId });
        }

    }
}
