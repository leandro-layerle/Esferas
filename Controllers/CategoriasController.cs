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
    public class CategoriasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public async Task<IActionResult> Subcategorias(int padreId)
        {
            var subcategorias = await _context.Categorias
                .Where(c => c.CategoriaPadreId == padreId)
                .Select(c => new
                {
                    c.Id,
                    c.Nombre
                })
                .ToListAsync();

            return Json(subcategorias);
        }

        public async Task<IActionResult> Index()
        {
            var categorias = await _context.Categorias
                .Include(c => c.CategoriaPadre)
                .ToListAsync();

            var jerarquicas = OrganizarJerarquia(categorias);

            return View(jerarquicas);
        }

        private List<Categoria> OrganizarJerarquia(List<Categoria> todas)
        {
            var resultado = new List<Categoria>();

            void AgregarHijos(Categoria padre, int nivel)
            {
                padre.Nombre = new string('─', nivel * 2) + " " + padre.Nombre;
                resultado.Add(padre);

                var hijos = todas.Where(c => c.CategoriaPadreId == padre.Id).ToList();
                foreach (var hijo in hijos)
                {
                    AgregarHijos(hijo, nivel + 1);
                }
            }

            var raices = todas.Where(c => c.CategoriaPadreId == null).ToList();
            foreach (var raiz in raices)
            {
                AgregarHijos(raiz, 0);
            }

            return resultado;
        }

        public async Task<IActionResult> Create()
        {
            var categorias = await _context.Categorias
                .OrderBy(c => c.Tipo)
                .ToListAsync();

            var opciones = categorias.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = (c.Tipo == TipoCategoria.Primaria ? "📁 " : "   ") + c.Nombre
            }).ToList();

            var viewModel = new CategoriaCreateEditViewModel
            {
                ListaCategorias = new SelectList(opciones, "Value", "Text")
            };

            return View(viewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoriaCreateEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.ListaCategorias = new SelectList(_context.Categorias.ToList(), "Id", "Nombre");
                return View(model);
            }

            var categoria = new Categoria
            {
                Nombre = model.Nombre,
                Tipo = model.Tipo,
                CategoriaPadreId = model.CategoriaPadreId
            };

            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            var model = new CategoriaCreateEditViewModel
            {
                Id = categoria.Id,
                Nombre = categoria.Nombre,
                Tipo = categoria.Tipo,
                CategoriaPadreId = categoria.CategoriaPadreId,
                ListaCategorias = new SelectList(_context.Categorias.Where(c => c.Id != id).ToList(), "Id", "Nombre")
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CategoriaCreateEditViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                model.ListaCategorias = new SelectList(_context.Categorias.Where(c => c.Id != id).ToList(), "Id", "Nombre");
                return View(model);
            }

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null) return NotFound();

            categoria.Nombre = model.Nombre;
            categoria.Tipo = model.Tipo;
            categoria.CategoriaPadreId = model.CategoriaPadreId;

            _context.Update(categoria);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _context.Categorias
                .Include(c => c.CategoriaPadre)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (categoria == null) return NotFound();

            return View(categoria);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
