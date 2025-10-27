using Esferas.Data;
using Esferas.Models.Entities;
using Esferas.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Esferas.Controllers
{
    [AllowAnonymous]
    public class WizardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WizardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Bienvenida(Guid id)
        {
            var link = await _context.LinksUnicos
                .Include(l => l.Encuesta)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (link == null || link.Estado != EstadoLink.Activo || link.Usado)
                return NotFound("Este link no es válido o ya fue utilizado.");

            ViewBag.LinkId = id;
            return View();
        }

        public async Task<IActionResult> Responder(Guid id, int paso = 1)
        {
            var link = await _context.LinksUnicos
                .Include(l => l.Encuesta)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (link == null || link.Estado != EstadoLink.Activo || link.Usado)
                return NotFound("Este link no es válido o ya fue utilizado.");

            var preguntas = await _context.Preguntas
                .Where(p => p.EncuestaId == link.EncuestaId)
                .OrderBy(p => p.Id)
                .ToListAsync();

            var preguntasRespondidas = await _context.Respuestas
                .Where(r => r.LinkUnicoId == link.Id)
                .Select(r => r.PreguntaId)
                .ToListAsync();

            var preguntasPendientes = preguntas
                .Where(p => !preguntasRespondidas.Contains(p.Id))
                .ToList();

            if (!preguntasPendientes.Any())
            {
                link.Usado = true;
                link.Estado = EstadoLink.Respondido;
                await _context.SaveChangesAsync();
                return RedirectToAction("Finalizado");
            }

            var preguntaActual = preguntasPendientes.First();
            var nuevoPaso = preguntas.IndexOf(preguntaActual) + 1;

            ViewBag.LinkId = id;
            ViewBag.Paso = nuevoPaso;
            ViewBag.Total = preguntas.Count;

            return View("Paso", preguntaActual);
        }


        [HttpPost]
        public async Task<IActionResult> Guardar(Guid id, int paso, int preguntaId, string respuestaTexto, int? respuestaNumerica, string justificacion)
        {
            var link = await _context.LinksUnicos.FindAsync(id);
            if (link == null || link.Usado) return NotFound();

            var pregunta = await _context.Preguntas.FindAsync(preguntaId);
            if (pregunta == null) return NotFound();

            var respuesta = new Respuesta
            {
                EncuestaId = link.EncuestaId,
                PreguntaId = preguntaId,
                LinkUnicoId = id,
                Fecha = DateTime.Now,
                RespuestaTexto = respuestaTexto,
                RespuestaNumerica = respuestaNumerica,
                Justificacion = justificacion
            };

            _context.Respuestas.Add(respuesta);
            await _context.SaveChangesAsync();

            var totalPreguntas = await _context.Preguntas.CountAsync(p => p.EncuestaId == link.EncuestaId);
            if (paso >= totalPreguntas)
            {
                link.Usado = true;
                link.Estado = Models.Enums.EstadoLink.Respondido;
                await _context.SaveChangesAsync();
                await GenerarResultados(link.EncuestaId);
                return RedirectToAction("Finalizado");
            }

            return RedirectToAction("Responder", new { id, paso = paso + 1 });
        }

        private async Task GenerarResultados(int encuestaId)
        {
            // Paso 1: Eliminar resultados previos
            var resultadosAnteriores = await _context.Resultados
                .Where(r => r.EncuestaId == encuestaId)
                .ToListAsync();

            _context.Resultados.RemoveRange(resultadosAnteriores);
            await _context.SaveChangesAsync();

            // Paso 2: Obtener respuestas relevantes
            var respuestas = await _context.Respuestas
                .Include(r => r.Pregunta)
                .Where(r => r.EncuestaId == encuestaId && r.Pregunta.EsRelevanteParaSemaforo && r.RespuestaNumerica.HasValue)
                .ToListAsync();

            var categorias = new[]
            {
                ("CategoriaPrimariaId", "Primaria"),
                ("CategoriaSecundariaId", "Secundaria"),
                ("CategoriaTerciariaId", "Terciaria")
            };

            foreach (var (prop, tipo) in categorias)
            {
                var agrupado = respuestas
                    .GroupBy(r =>
                    {
                        return prop switch
                        {
                            "CategoriaPrimariaId" => r.Pregunta.CategoriaPrimariaId,
                            "CategoriaSecundariaId" => r.Pregunta.CategoriaSecundariaId,
                            "CategoriaTerciariaId" => r.Pregunta.CategoriaTerciariaId,
                            _ => null
                        };
                    })
                    .Where(g => g.Key.HasValue)
                    .Select(g => new
                    {
                        CategoriaId = g.Key.Value,
                        Promedio = g.Average(r => r.RespuestaNumerica.Value)
                    });

                foreach (var dato in agrupado)
                {
                    _context.Resultados.Add(new Resultado
                    {
                        EncuestaId = encuestaId,
                        CategoriaId = dato.CategoriaId,
                        Promedio = dato.Promedio,
                        Color = GetColor(dato.Promedio)
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        private string GetColor(double promedio)
        {
            if (promedio <= 2.5) return SemaforoColor.Rojo;
            if (promedio <= 3.5) return SemaforoColor.Amarillo;
            return SemaforoColor.Verde;
        }

        public IActionResult Finalizado()
        {
            return View();
        }
    }
}
