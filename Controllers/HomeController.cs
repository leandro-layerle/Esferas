using Esferas.Data;
using Esferas.Models.DTOs;
using Esferas.Models.Enums;
using Esferas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Esferas.Controllers
{
   
    public class HomeController : Controller
    {
        private readonly DashboardService _dashboardService;
        private readonly ApplicationDbContext _context;
        public HomeController(ApplicationDbContext context,DashboardService dashboardService)
        {
            _context = context;
            _dashboardService = dashboardService;
        }

        [Authorize]
        public async Task<IActionResult> Dashboard()
        {
           return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEmpresas()
        {
            var empresas = await _context.Empresas
                .Select(e => new { e.Id, e.Nombre })
                .ToListAsync();

            return Json(empresas);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEncuestasPorEmpresa(int empresaId)
        {
            var encuestas = await _context.Encuestas
                .Where(e => e.EmpresaId == empresaId)
                .Select(e => new { e.Id })
                .ToListAsync();

            return Json(encuestas);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerKpis([FromQuery] DashboardFiltrosDto filtros)
        {
            var data = await _dashboardService.ObtenerKpisAsync(filtros);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDistribucionSemaforo([FromQuery] DashboardFiltrosDto filtros)
        {
            var data = await _dashboardService.ObtenerDistribucionSemaforoAsync(filtros);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerPromediosPorEsfera([FromQuery] DashboardFiltrosDto filtros)
        {
            var data = await _dashboardService.ObtenerPromediosPorEsferaAsync(filtros);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerEsferasRojas([FromQuery] int? empresaId, [FromQuery] int? encuestaId)
        {
            var query = _context.Resultados
                .Include(r => r.Encuesta)
                .Include(r => r.Categoria)
                .AsQueryable();

            if (empresaId.HasValue)
            {
                query = query.Where(r => r.Encuesta.EmpresaId == empresaId);
            }

            if (encuestaId.HasValue)
            {
                query = query.Where(r => r.EncuestaId == encuestaId);
            }

            var esferasEnRojo = await query
                .Where(r => r.Color == SemaforoColor.Rojo)
                .Select(r => new
                {
                    esfera = r.Categoria.Nombre,
                    promedio = r.Promedio,
                    encuesta = r.Encuesta.Id
                })
                .OrderBy(r => r.promedio)
                .ToListAsync();

            return Json(esferasEnRojo);
        }



        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
