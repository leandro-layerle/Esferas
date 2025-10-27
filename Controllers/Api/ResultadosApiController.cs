using Esferas.Data;
using Esferas.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Esferas.Controllers.Api
{
    [ApiController]
    [Route("api/resultados")]
    public class ResultadosApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ResultadosApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{encuestaId}")]
        public async Task<IActionResult> GetDashboard(int encuestaId)
        {
            var resultados = await _context.Resultados
                .Include(r => r.Categoria)
                .Where(r => r.EncuestaId == encuestaId)
                .ToListAsync();

            // Organizar en árbol
            var primarias = resultados
                .Where(r => r.Categoria.Tipo == Models.Enums.TipoCategoria.Primaria)
                .Select(r => new ResultadoNodoDto
                {
                    Name = r.Categoria.Nombre,
                    Promedio = r.Promedio,
                    Color = GetColor(r.Promedio),
                    Children = resultados
                        .Where(s => s.Categoria.CategoriaPadreId == r.Categoria.Id)
                        .Select(s => new ResultadoNodoDto
                        {
                            Name = s.Categoria.Nombre,
                            Promedio = s.Promedio,
                            Color = GetColor(s.Promedio),
                            Children = resultados
                                .Where(t => t.Categoria.CategoriaPadreId == s.Categoria.Id)
                                .Select(t => new ResultadoNodoDto
                                {
                                    Name = t.Categoria.Nombre,
                                    Promedio = t.Promedio,
                                    Color = GetColor(t.Promedio)
                                }).ToList()
                        }).ToList()
                }).ToList();

            var promedioGlobal = primarias.Any()
                ? primarias.Average(p => p.Promedio)
                : 0.0;

            var root = new ResultadoNodoDto
            {
                Name = "Resultados",
                Promedio = promedioGlobal,
                Color = GetColor(promedioGlobal),
                Children = primarias
            };

            return Ok(root);
        }

        private static string GetColor(double promedio)
        {
            if (promedio <= 2.5) return "#dc3545";       // Rojo
            if (promedio <= 3.5) return "#ffc107";       // Amarillo
            return "#28a745";                            // Verde
        }
    }
}
