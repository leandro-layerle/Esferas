using Esferas.Data;
using Esferas.Models.DTOs;
using Esferas.Models.Entities;
using Esferas.Models.Enums;
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
        private readonly IConfiguration _config;
        private readonly InformeMistralService _informeMistralService;

        public EmpresasController(ApplicationDbContext context, ResultadosEmpresaService resultadosService, IConfiguration config, InformeMistralService informeMistralService)
        {
            _context = context;
            _resultadosService = resultadosService;
            _config = config;
            _informeMistralService = informeMistralService;
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

        [HttpGet("/empresa/esferas-secundarias")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEsferasSecundarias(Guid token, int categoriaPrimariaId)
        {
            var encuestaId = await _context.LinksUnicos
                .Where(l => l.Token == token && l.EsLinkEmpresa)
                .Select(l => l.EncuestaId)
                .FirstOrDefaultAsync();

            if (encuestaId == 0)
                return BadRequest("Token inválido o no relacionado con empresa.");

            var umbralRojo = _config.GetValue<double>("UmbralesSemaforo:RojoHasta");
            var umbralAmarillo = _config.GetValue<double>("UmbralesSemaforo:AmarilloHasta");

            var secundarias = await _context.Resultados
                .Where(r => r.EncuestaId == encuestaId)
                .Join(_context.Categorias, r => r.CategoriaId, c => c.Id, (r, c) => new { r, c })
                .Where(x => x.c.Tipo == TipoCategoria.Secundaria && x.c.CategoriaPadreId == categoriaPrimariaId)
                .Select(x => new EsferaSecundariaDto
                {
                    Nombre = x.c.Nombre,
                    Promedio = x.r.Promedio,
                    ColorSemaforo = ResultadosEmpresaService.CalcularColorSemaforo(
                        x.r.Promedio, umbralRojo, umbralAmarillo)
                })
                .ToListAsync();

            return Json(secundarias);
        }

        [HttpGet("/empresa/r/{token}/informe")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerarInforme(Guid token)
        {
            var viewModel = await _resultadosService.ObtenerResultadosPorTokenAsync(token);
            if (viewModel == null)
                return NotFound("No se encontraron resultados para el token proporcionado.");

            var informe = await _informeMistralService.GenerarInformeAsync(viewModel.EsferasPrimarias, viewModel.PromedioGeneral);

            return Content(informe, "text/plain", System.Text.Encoding.UTF8);
        }


    }
}
