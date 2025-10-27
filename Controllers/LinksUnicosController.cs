using Esferas.Data;
using Esferas.Models.Entities;
using Esferas.Models.Enums;
using Esferas.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Esferas.Controllers
{
    [Authorize]
    public class LinksUnicosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LinksUnicosController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int encuestaId)
        {
            var links = await _context.LinksUnicos
                .Where(l => l.EncuestaId == encuestaId)
                .ToListAsync();

            ViewBag.EncuestaId = encuestaId;
            return View(links);
        }

        public IActionResult Create(int encuestaId)
        {
            var model = new LinkUnicoViewModel
            {
                EncuestaId = encuestaId,
                FechaExpiracion = DateTime.Now.AddDays(7),
                ListaEstados = new SelectList(Enum.GetValues(typeof(EstadoLink)))
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LinkUnicoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            for (int i = 0; i < model.Cantidad; i++)
            {
                var link = new LinkUnico
                {
                    Id = Guid.NewGuid(),
                    EncuestaId = model.EncuestaId,
                    FechaExpiracion = model.FechaExpiracion,
                    Estado = EstadoLink.Activo,
                    Usado = false
                };

                _context.LinksUnicos.Add(link);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { encuestaId = model.EncuestaId });
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var link = await _context.LinksUnicos.FindAsync(id);
            if (link == null) return NotFound();

            return View(link);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var link = await _context.LinksUnicos.FindAsync(id);
            var encuestaId = link.EncuestaId;

            _context.LinksUnicos.Remove(link);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { encuestaId });
        }
    }
}
