using Esferas.Data;
using Esferas.Models.Entities;
using Esferas.Models.ViewModels;
using Esferas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Esferas.Controllers
{
    [Authorize]
    public class EmpresasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ResultadosEmpresaService _resultadosService;

        public EmpresasController(ApplicationDbContext context, ResultadosEmpresaService resultadosService)
        {
            _context = context;
            _resultadosService = resultadosService;
        }


        public async Task<IActionResult> Index()
        {
            var empresas = await _context.Empresas.ToListAsync();
            return View(empresas);
        }

        public IActionResult Create()
        {
            return View(new EmpresaViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmpresaViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var empresa = new Empresa
            {
                Nombre = model.Nombre,
                Cultura = model.Cultura
            };

            _context.Empresas.Add(empresa);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null) return NotFound();

            var model = new EmpresaViewModel
            {
                Id = empresa.Id,
                Nombre = empresa.Nombre,
                Cultura = empresa.Cultura
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmpresaViewModel model)
        {
            if (id != model.Id) return NotFound();
            if (!ModelState.IsValid) return View(model);

            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null) return NotFound();

            empresa.Nombre = model.Nombre;
            empresa.Cultura = model.Cultura;

            _context.Update(empresa);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            if (empresa == null) return NotFound();

            return View(empresa);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var empresa = await _context.Empresas.FindAsync(id);
            _context.Empresas.Remove(empresa);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/empresa/r/{token}")]
        [AllowAnonymous]
        public async Task<IActionResult> Resultados(Guid token)
        {
            var viewModel = await _resultadosService.ObtenerResultadosPorTokenAsync(token);
            if (viewModel == null)
                return NotFound();

            return View("Resultados", viewModel);
        }

    }
}
