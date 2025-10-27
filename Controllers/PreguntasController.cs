using Esferas.Data;
using Esferas.Models.Entities;
using Esferas.Models.Enums;
using Esferas.Models.ViewModels;
using Esferas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Esferas.Controllers
{
    [Authorize]
    public class PreguntasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PreguntaImportService _preguntaImportService;

        public PreguntasController(ApplicationDbContext context, PreguntaImportService preguntaImportService)
        {
            _context = context;
            _preguntaImportService = preguntaImportService;
        }

        public async Task<IActionResult> Index(int encuestaId)
        {
            var preguntas = await _context.Preguntas
                .Include(p => p.CategoriaPrimaria)
                .Include(p => p.CategoriaSecundaria)
                .Include(p => p.CategoriaTerciaria)
                .Where(p => p.EncuestaId == encuestaId)
                .ToListAsync();

            var encuesta = await _context.Encuestas
            .Include(e => e.Empresa)
            .FirstOrDefaultAsync(e => e.Id == encuestaId);

            if (encuesta == null) return NotFound();

            ViewBag.EncuestaId = encuestaId;
            ViewBag.EncuestaNombre = $"Encuesta del {encuesta.FechaCreacion:dd/MM/yyyy} - {encuesta.Empresa?.Nombre}";
            return View(preguntas);
        }

        public IActionResult Create(int encuestaId)
        {
            var model = new PreguntaViewModel
            {
                EncuestaId = encuestaId,
                CategoriasPrimarias = GetCategorias(TipoCategoria.Primaria),
                CategoriasSecundarias = GetCategorias(TipoCategoria.Secundaria),
                CategoriasTerciarias = GetCategorias(TipoCategoria.Terciaria)
            };


            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PreguntaViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.CategoriasPrimarias = GetCategorias(TipoCategoria.Primaria);
                model.CategoriasSecundarias = GetCategorias(TipoCategoria.Secundaria);
                model.CategoriasTerciarias = GetCategorias(TipoCategoria.Terciaria);
                return View(model);
            }

            var pregunta = new Pregunta
            {
                EncuestaId = model.EncuestaId,
                Texto = model.Texto,
                TipoRespuesta = model.TipoRespuesta,
                CategoriaPrimariaId = model.CategoriaPrimariaId,
                CategoriaSecundariaId = model.CategoriaSecundariaId,
                CategoriaTerciariaId = model.CategoriaTerciariaId,
                EsPersonalizable = model.EsPersonalizable,
                EsRelevanteParaSemaforo = model.EsRelevanteParaSemaforo
            };

            _context.Preguntas.Add(pregunta);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { encuestaId = model.EncuestaId });
        }

        public async Task<IActionResult> Edit(int id)
        {
            var pregunta = await _context.Preguntas.FindAsync(id);
            if (pregunta == null) return NotFound();

            var model = new PreguntaViewModel
            {
                Id = pregunta.Id,
                EncuestaId = pregunta.EncuestaId,
                Texto = pregunta.Texto,
                TipoRespuesta = pregunta.TipoRespuesta,
                CategoriaPrimariaId = pregunta.CategoriaPrimariaId,
                CategoriaSecundariaId = pregunta.CategoriaSecundariaId,
                CategoriaTerciariaId = pregunta.CategoriaTerciariaId,
                EsPersonalizable = pregunta.EsPersonalizable,
                EsRelevanteParaSemaforo = pregunta.EsRelevanteParaSemaforo,
                CategoriasPrimarias = GetCategorias(TipoCategoria.Primaria),
                CategoriasSecundarias = GetCategorias(TipoCategoria.Secundaria),
                CategoriasTerciarias = GetCategorias(TipoCategoria.Terciaria)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PreguntaViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid)
            {
                model.CategoriasPrimarias = GetCategorias(TipoCategoria.Primaria);
                model.CategoriasSecundarias = GetCategorias(TipoCategoria.Secundaria);
                model.CategoriasTerciarias = GetCategorias(TipoCategoria.Terciaria);
                return View(model);
            }

            var pregunta = await _context.Preguntas.FindAsync(id);
            if (pregunta == null) return NotFound();

            pregunta.Texto = model.Texto;
            pregunta.TipoRespuesta = model.TipoRespuesta;
            pregunta.CategoriaPrimariaId = model.CategoriaPrimariaId;
            pregunta.CategoriaSecundariaId = model.CategoriaSecundariaId;
            pregunta.CategoriaTerciariaId = model.CategoriaTerciariaId;
            pregunta.EsPersonalizable = model.EsPersonalizable;
            pregunta.EsRelevanteParaSemaforo = model.EsRelevanteParaSemaforo;

            _context.Update(pregunta);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { encuestaId = model.EncuestaId });
        }

        public async Task<IActionResult> Delete(int id)
        {
            var pregunta = await _context.Preguntas
                .Include(p => p.CategoriaPrimaria)
                .Include(p => p.CategoriaSecundaria)
                .Include(p => p.CategoriaTerciaria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (pregunta == null) return NotFound();

            return View(pregunta);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var pregunta = await _context.Preguntas.FindAsync(id);
            int encuestaId = pregunta.EncuestaId;

            _context.Preguntas.Remove(pregunta);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { encuestaId });
        }

        [HttpPost]
        public async Task<IActionResult> ImportarDesdeArchivo(IFormFile archivoExcel, int encuestaId)
        {
            using var stream = archivoExcel.OpenReadStream();
            var (logs, fueExito) = await _preguntaImportService.ImportarDesdeExcelAsync(stream, encuestaId);
            return Json(new { logs, fueExito });
        }

        private SelectList GetCategorias(TipoCategoria tipo)
        {
            return new SelectList(_context.Categorias
                .Where(c => c.Tipo == tipo)
                .OrderBy(c => c.Nombre)
                .ToList(), "Id", "Nombre");
        }
    }
}
