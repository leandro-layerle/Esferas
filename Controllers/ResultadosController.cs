using Esferas.Data;
using Esferas.Models.DTOs;
using Esferas.Models.Enums;
using Esferas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Esferas.Controllers
{
    [AllowAnonymous]
    public class ResultadosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ResultadosEmpresaService _resultadosEmpresaService;

        public ResultadosController(ApplicationDbContext context, ResultadosEmpresaService resultadosEmpresaService)
        {
            _context = context;
            _resultadosEmpresaService = resultadosEmpresaService;
        }

        public IActionResult Index(int encuestaId)
        {
            ViewBag.EncuestaId = encuestaId;
            return View();
        }

        [AllowAnonymous]
        [HttpGet("/api/resultados/esferas-secundarias")]
        public async Task<IActionResult> ObtenerEsferasSecundarias(int encuestaId, int? primariaId)
        {
            var datos = await _resultadosEmpresaService.ObtenerEsferasSecundariasAsync(encuestaId, primariaId);
            return Ok(datos);
        }

        //// URL: /r/{token:guid}
        //[HttpGet]
        //public async Task<IActionResult> Personal(Guid token)
        //{
        //    // Suponiendo una entidad LinkUnico con Token (Guid) y referencia a Encuesta
        //    var link = await _context.LinksUnicos
        //        .Include(l => l.Encuesta)
        //        .FirstOrDefaultAsync(l => l.Token == token);

        //    if (link is null) return NotFound(); // token inválido

        //    var encuesta = link.Encuesta;
        //    var encuestaId = encuesta.Id;

        //    // Resultados por esfera primaria (Categoria.Tipo = Primaria)
        //    var resultados = await _context.Resultados
        //        .Include(r => r.Categoria)
        //        .Where(r => r.EncuestaId == encuestaId && r.Categoria.Tipo == TipoCategoria.Primaria)
        //        .ToListAsync();

        //    // Promedio general desde Respuestas (más preciso)
        //    var promedioGeneral = 0d;
        //    var hayNumericas = await _context.Respuestas
        //        .AnyAsync(r => r.EncuestaId == encuestaId && r.RespuestaNumerica.HasValue);

        //    if (hayNumericas)
        //    {
        //        promedioGeneral = await _context.Respuestas
        //            .Where(r => r.EncuestaId == encuestaId && r.RespuestaNumerica.HasValue)
        //            .AverageAsync(r => r.RespuestaNumerica!.Value);
        //    }

        //    var model = new PersonalDashboardDto
        //    {
        //        NombrePersona = link.NombreMostrable ?? "Tu informe",
        //        NombreEncuesta = encuesta.Nombre,
        //        Fecha = link.FechaFinalizacion ?? DateTime.UtcNow,
        //        PromedioGeneral = Math.Round(promedioGeneral, 2),

        //        CantRojas = resultados.Count(r => r.Color == "Rojo"),
        //        CantAmarillas = resultados.Count(r => r.Color == "Amarillo"),
        //        CantVerdes = resultados.Count(r => r.Color == "Verde"),

        //        RadarEsferas = resultados
        //            .OrderBy(r => r.Categoria.Nombre)
        //            .Select(r => (r.Categoria.Nombre, Math.Round(r.Promedio, 2)))
        //            .ToList(),

        //        EsferasRojas = resultados
        //            .Where(r => r.Color == "Rojo")
        //            .OrderBy(r => r.Promedio)
        //            .Select(r => (r.Categoria.Nombre, Math.Round(r.Promedio, 2), (string?)null))
        //            .ToList(),

        //        EsferasAmarillas = resultados
        //            .Where(r => r.Color == "Amarillo")
        //            .OrderBy(r => r.Promedio)
        //            .Select(r => (r.Categoria.Nombre, Math.Round(r.Promedio, 2), (string?)null))
        //            .ToList(),

        //        EsferasVerdes = resultados
        //            .Where(r => r.Color == "Verde")
        //            .OrderByDescending(r => r.Promedio)
        //            .Select(r => (r.Categoria.Nombre, Math.Round(r.Promedio, 2), (string?)null))
        //            .ToList(),
        //    };

        //    // TODO (opcional): poblar Recomendaciones según reglas o tabla
        //    // model.Recomendaciones = await _recoService.ObtenerPara(encuestaId, model.EsferasRojas.Select(x => x.Esfera));

        //    return View("Personal", model); // Usa la vista que ya armamos
        //}
    }
}
