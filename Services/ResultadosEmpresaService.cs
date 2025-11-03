using Esferas.Data;
using Esferas.Models;
using Esferas.Models.DTOs;
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
            var link = await _context.LinksUnicos
                .Include(l => l.Encuesta)
                .FirstOrDefaultAsync(l => l.Token == token);

            if (link == null || link.Encuesta == null)
                return null;

            var encuestaId = link.EncuestaId;

            // Promedio general
            var respuestas = await _context.Respuestas
                .Where(r => r.EncuestaId == encuestaId && r.RespuestaNumerica.HasValue)
                .Join(_context.Preguntas,
                    r => r.PreguntaId,
                    p => p.Id,
                    (r, p) => new { r.RespuestaNumerica, p.EsRelevanteParaSemaforo})
                .Where(x => x.EsRelevanteParaSemaforo)
                .ToListAsync();

            double promedioGeneral = respuestas.Any() ? respuestas.Average(x => x.RespuestaNumerica.Value) : 0;
            string colorGeneral = CalcularColorSemaforo(promedioGeneral);

            // Promedios por esfera primaria
            var resultados = await _context.Resultados
                .Where(r => r.EncuestaId == encuestaId)
                .Join(_context.Categorias,
                    r => r.CategoriaId,
                    c => c.Id,
                    (r, c) => new { r, c })
                .Where(x => x.c.Tipo == Models.Enums.TipoCategoria.Primaria)
                .Select(x => new EsferaResultadoDto
                {
                    CategoriaId = x.c.Id,
                    Nombre = x.c.Nombre,
                    Promedio = x.r.Promedio,
                    ColorSemaforo = CalcularColorSemaforo(x.r.Promedio)
                })
                .ToListAsync();

            return new EmpresaResultadosViewModel
            {
                PromedioGeneral = promedioGeneral,
                ColorPromedioGeneral = colorGeneral,
                EsferasPrimarias = resultados
            };
        }

        private string CalcularColorSemaforo(double promedio)
        {
            double rojo = _config.GetValue<double>("UmbralesSemaforo:RojoHasta");
            double amarillo = _config.GetValue<double>("UmbralesSemaforo:AmarilloHasta");

            if (promedio <= rojo)
                return SemaforoColor.Rojo;
            else if (promedio <= amarillo)
                return SemaforoColor.Amarillo;
            else
                return SemaforoColor.Verde;
        }
    }
}
