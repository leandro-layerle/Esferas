using Esferas.Data;
using Esferas.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Esferas.Controllers.Api
{
    [Route("api/informes")]
    [ApiController]
    [AllowAnonymous]
    public class InformesController : ControllerBase
    {
        private readonly InformeMistralService _informeService;
        private readonly ResultadosEmpresaService _resultadosService;
        private readonly InformePdfService _pdfService;
        private readonly ApplicationDbContext _context;

        public InformesController(InformeMistralService informeService, ResultadosEmpresaService resultadosService, InformePdfService pdfService, ApplicationDbContext context)
        {
            _informeService = informeService;
            _resultadosService = resultadosService;
            _pdfService = pdfService;
            _context = context;
        }

        [HttpGet("generar")]
        [AllowAnonymous]
        public async Task<IActionResult> GenerarInforme([FromQuery] Guid token)
        {
            var link = await _context.LinksUnicos
                .Include(l => l.Encuesta)
                .FirstOrDefaultAsync(l => l.Token == token);

            if (link == null)
                return NotFound("Token inválido.");

            var vm = await _resultadosService.ObtenerResultadosPorTokenAsync(token);
            if (vm == null)
                return NotFound("No se encontraron resultados.");

            var html = await _informeService.GenerarInformeConCacheAsync(
                link.EncuestaId,
                token,
                vm.EsferasPrimarias,
                vm.PromedioGeneral
            );

            return Content(html, "text/html");
        }

        //[HttpGet("pdf")]
        //[AllowAnonymous]
        //public async Task<IActionResult> GenerarInformePdf(Guid token)
        //{
        //    var viewModel = await _resultadosService.ObtenerResultadosPorTokenAsync(token);
        //    if (viewModel == null)
        //        return NotFound();

        //    // 1️⃣ Generar informe HTML usando Mistral
        //    var informeHtml = await _informeService.GenerarInformeAsync(viewModel.EsferasPrimarias, viewModel.PromedioGeneral);

        //    // 2️⃣ Convertir a PDF
        //    var pdfBytes = _pdfService.GenerarInformePdf(
        //        informeHtml,
        //        "Empresa Ejemplo", // Si tenés nombre de empresa, reemplazalo
        //        viewModel.PromedioGeneral
        //    );

        //    // 3️⃣ Devolver PDF al navegador
        //    return File(pdfBytes, "application/pdf", $"Informe_{DateTime.Now:yyyyMMdd}.pdf");
        //}

    }
}
