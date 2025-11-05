using Esferas.Data;
using Esferas.Models;
using Esferas.Models.DTOs;
using Esferas.Models.Entities;
using Esferas.Models.Enums;
using Esferas.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esferas.Services
{
    public class ResultadosEmpresaService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;

        public ResultadosEmpresaService(ApplicationDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<EmpresaResultadosViewModel> ObtenerResultadosPorTokenAsync(Guid token)
        {
            var encuestaId = await _context.LinksUnicos
                .Where(l => l.Token == token)
                .Select(l => l.EncuestaId)
                .FirstOrDefaultAsync();

            if (encuestaId == 0)
                return null;

            // Umbrales desde configuración
            var umbralRojo = _config.GetValue<double>("UmbralesSemaforo:RojoHasta");
            var umbralAmarillo = _config.GetValue<double>("UmbralesSemaforo:AmarilloHasta");

            // Obtener resultados por esfera primaria
            var resultados = await _context.Resultados
                .Where(r => r.EncuestaId == encuestaId)
                .Join(
                    _context.Categorias.Where(c => c.Tipo == TipoCategoria.Primaria),
                    r => r.CategoriaId,
                    c => c.Id,
                    (r, c) => new { c.Id, c.Nombre, r.Promedio }
                )
                .ToListAsync();

            // Calcular promedio general desde los resultados primarios
            var promedioGeneral = resultados.Any() ? resultados.Average(x => x.Promedio) : 0;

            var colorGeneral = CalcularColorSemaforo(promedioGeneral, umbralRojo, umbralAmarillo);

            var dtoList = resultados
                .Select(x => new EsferaResultadoDto
                {
                    CategoriaId = x.Id,
                    Nombre = x.Nombre,
                    Promedio = x.Promedio,
                    ColorSemaforo = CalcularColorSemaforo(x.Promedio, umbralRojo, umbralAmarillo)
                })
                .ToList();

            return new EmpresaResultadosViewModel
            {
                PromedioGeneral = promedioGeneral,
                ColorPromedioGeneral = colorGeneral,
                EsferasPrimarias = dtoList,
                EncuestaId = encuestaId,
                Token = token
            };
        }

        public async Task<List<EsferaResultadoDto>> ObtenerEsferasSecundariasAsync(int encuestaId, int? primariaId = null)
        {
            var umbralRojo = _config.GetValue<double>("UmbralesSemaforo:RojoHasta");
            var umbralAmarillo = _config.GetValue<double>("UmbralesSemaforo:AmarilloHasta");

            var query = _context.Resultados
                .Where(r => r.EncuestaId == encuestaId)
                .Join(_context.Categorias.Where(c => c.Tipo == TipoCategoria.Secundaria),
                    r => r.CategoriaId,
                    c => c.Id,
                    (r, c) => new { Categoria = c, r.Promedio });

            // Filtrar por primaria si se indica
            if (primariaId.HasValue && primariaId.Value > 0)
            {
                query = query.Where(x => x.Categoria.CategoriaPadreId == primariaId.Value);
            }

            var datos = await query
                .Select(x => new EsferaResultadoDto
                {
                    CategoriaId = x.Categoria.Id,
                    Nombre = x.Categoria.Nombre,
                    Promedio = x.Promedio,
                    ColorSemaforo = CalcularColorSemaforo(x.Promedio, umbralRojo, umbralAmarillo)
                })
                .ToListAsync();

            return datos;
        }


        // Método ahora estático para evitar memory leak
        public static string CalcularColorSemaforo(double promedio, double umbralRojo, double umbralAmarillo)
        {
            if (promedio <= umbralRojo)
                return SemaforoColor.Rojo;
            else if (promedio <= umbralAmarillo)
                return SemaforoColor.Amarillo;
            else
                return SemaforoColor.Verde;
        }

        public async Task<EmpresaResultadosViewModel> ObtenerResultadosPorEncuestaIdAsync(int encuestaId)
        {
            // Umbrales desde configuración
            var umbralRojo = _config.GetValue<double>("UmbralesSemaforo:RojoHasta");
            var umbralAmarillo = _config.GetValue<double>("UmbralesSemaforo:AmarilloHasta");

            // Obtener resultados por esfera primaria
            var resultados = await _context.Resultados
                .Where(r => r.EncuestaId == encuestaId)
                .Join(
                    _context.Categorias.Where(c => c.Tipo == TipoCategoria.Primaria),
                    r => r.CategoriaId,
                    c => c.Id,
                    (r, c) => new { c.Id, c.Nombre, r.Promedio }
                )
                .ToListAsync();

            // Calcular promedio general desde los resultados primarios
            var promedioGeneral = resultados.Any() ? resultados.Average(x => x.Promedio) : 0;

            var colorGeneral = ResultadosEmpresaService.CalcularColorSemaforo(promedioGeneral, umbralRojo, umbralAmarillo);

            var dtoList = resultados
                .Select(x => new EsferaResultadoDto
                {
                    CategoriaId = x.Id,
                    Nombre = x.Nombre,
                    Promedio = x.Promedio,
                    ColorSemaforo = ResultadosEmpresaService.CalcularColorSemaforo(x.Promedio, umbralRojo, umbralAmarillo)
                })
                .ToList();

            return new EmpresaResultadosViewModel
            {
                PromedioGeneral = promedioGeneral,
                ColorPromedioGeneral = colorGeneral,
                EsferasPrimarias = dtoList,
                EncuestaId = encuestaId
            };
        }

    }
}
