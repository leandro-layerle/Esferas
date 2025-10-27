using Esferas.Data;
using Esferas.Models.DTOs;
using Esferas.Models.Enums;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;

namespace Esferas.Services
{
    public class DashboardService
    {
        private readonly ApplicationDbContext _context;
        public DashboardService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardKpiDto> ObtenerKpisAsync()
        {
            var totalEncuestas = await _context.Encuestas.CountAsync();

            var encuestasCompletadas = await _context.Encuestas
                .CountAsync(c => c.Estado == Models.Enums.EstadoEncuesta.Finalizada);

            var promedio = await _context.Respuestas
                .Where(r => r.RespuestaNumerica.HasValue)
                .AverageAsync(r => r.RespuestaNumerica);

            if (!promedio.HasValue) promedio = 0; 

            var esferasRojas = await _context.Resultados
                .CountAsync(c => c.Color == SemaforoColor.Rojo);

            var kpis = new DashboardKpiDto
            {
                TotalEncuestas = totalEncuestas,
                EncuestasCompletadas = encuestasCompletadas,
                PromedioEfectividad = Math.Round(promedio.Value, 2),
                EsferasEnRojo = esferasRojas
            };

            return kpis;

        }

        public async Task<DistribucionSemaforoDto> ObtenerDistribucionSemaforoAsync()
        {
            var rojo = await _context.Resultados.CountAsync(r => r.Color == SemaforoColor.Rojo);
            var amarillo = await _context.Resultados.CountAsync(r => r.Color == SemaforoColor.Amarillo);
            var verde = await _context.Resultados.CountAsync(r => r.Color == SemaforoColor.Verde);

            return new DistribucionSemaforoDto
            {
                Rojo = rojo,
                Amarillo = amarillo,
                Verde = verde
            };
        }

        public async Task<List<PromedioEsferaDto>> ObtenerPromediosPorEsferaAsync()
        {
            // Suponiendo que cada Resultado tiene: CategoriaId, Categoria.Nombre, Categoria.Tipo = Primaria
            var promedios = await _context.Resultados
                .Where(r => r.Categoria.Tipo == TipoCategoria.Primaria)
                .GroupBy(r => r.Categoria.Nombre)
                .Select(g => new PromedioEsferaDto
                {
                    Esfera = g.Key,
                    Promedio = Math.Round(g.Average(r => r.Promedio), 2)
                })
                .ToListAsync();

            return promedios;
        }

        public async Task<DashboardKpiDto> ObtenerKpisAsync(DashboardFiltrosDto filtros)
        {
            var encuestasQuery = _context.Encuestas.AsQueryable();

            if (filtros.EmpresaId.HasValue)
                encuestasQuery = encuestasQuery.Where(e => e.EmpresaId == filtros.EmpresaId);

            if (filtros.EncuestaId.HasValue)
                encuestasQuery = encuestasQuery.Where(e => e.Id == filtros.EncuestaId);

            var encuestaIds = await encuestasQuery.Select(e => e.Id).ToListAsync();

            var respuestasQuery = _context.Respuestas
                .Where(r => encuestaIds.Contains(r.EncuestaId));

            var resultadosQuery = _context.Resultados
                .Where(r => encuestaIds.Contains(r.EncuestaId));

            var totalEncuestas = encuestaIds.Count;

            var encuestasCompletadas = await _context.Encuestas
                .Where(e => encuestaIds.Contains(e.Id) && e.Estado == EstadoEncuesta.Finalizada)
                .CountAsync();

            double promedio = 0;
            if (await respuestasQuery.AnyAsync(r => r.RespuestaNumerica.HasValue))
            {
                promedio = await respuestasQuery
                    .Where(r => r.RespuestaNumerica.HasValue)
                    .AverageAsync(r => r.RespuestaNumerica.Value);
            }

            var esferasRojas = await resultadosQuery.CountAsync(r => r.Color == SemaforoColor.Rojo);

            return new DashboardKpiDto
            {
                TotalEncuestas = totalEncuestas,
                EncuestasCompletadas = encuestasCompletadas,
                PromedioEfectividad = Math.Round(promedio, 2),
                EsferasEnRojo = esferasRojas
            };
        }

        public async Task<DistribucionSemaforoDto> ObtenerDistribucionSemaforoAsync(DashboardFiltrosDto filtros)
        {
            var encuestasQuery = _context.Encuestas.AsQueryable();

            if (filtros.EmpresaId.HasValue)
                encuestasQuery = encuestasQuery.Where(e => e.EmpresaId == filtros.EmpresaId);

            if (filtros.EncuestaId.HasValue)
                encuestasQuery = encuestasQuery.Where(e => e.Id == filtros.EncuestaId);

            var encuestaIds = await encuestasQuery.Select(e => e.Id).ToListAsync();

            var resultadosQuery = _context.Resultados
                .Where(r => encuestaIds.Contains(r.EncuestaId));

            var rojo = await resultadosQuery.CountAsync(r => r.Color == SemaforoColor.Rojo);
            var amarillo = await resultadosQuery.CountAsync(r => r.Color == SemaforoColor.Amarillo);
            var verde = await resultadosQuery.CountAsync(r => r.Color == SemaforoColor.Verde);

            return new DistribucionSemaforoDto
            {
                Rojo = rojo,
                Amarillo = amarillo,
                Verde = verde
            };
        }

        public async Task<List<PromedioEsferaDto>> ObtenerPromediosPorEsferaAsync(DashboardFiltrosDto filtros)
        {
            var encuestasQuery = _context.Encuestas.AsQueryable();

            if (filtros.EmpresaId.HasValue)
                encuestasQuery = encuestasQuery.Where(e => e.EmpresaId == filtros.EmpresaId);

            if (filtros.EncuestaId.HasValue)
                encuestasQuery = encuestasQuery.Where(e => e.Id == filtros.EncuestaId);

            var encuestaIds = await encuestasQuery.Select(e => e.Id).ToListAsync();

            var resultadosQuery = _context.Resultados
                .Where(r => encuestaIds.Contains(r.EncuestaId) && r.Categoria.Tipo == TipoCategoria.Primaria);

            var promedios = await resultadosQuery
                .GroupBy(r => r.Categoria.Nombre)
                .Select(g => new PromedioEsferaDto
                {
                    Esfera = g.Key,
                    Promedio = Math.Round(g.Average(r => r.Promedio), 2)
                })
                .ToListAsync();

            return promedios;
        }



    }

}
