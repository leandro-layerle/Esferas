using Esferas.Data;
using Esferas.Models.DTOs;
using Esferas.Models.Entities;
using Esferas.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Esferas.Services
{
    public class InformeMistralService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _context;

        public InformeMistralService(HttpClient httpClient, IConfiguration config, ApplicationDbContext context)
        {
            _httpClient = httpClient;
            _config = config;
            _context = context;
        }

        public async Task<string> GenerarInformeAsync(List<EsferaResultadoDto> esferas, double promedioGeneral)
        {
            var apiKey = _config["Mistral:ApiKey"];
            var baseUrl = _config["Mistral:BaseUrl"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("API key de Mistral no configurada.");

            var prompt = ConstruirPrompt(esferas, promedioGeneral);

            var request = new
            {
                model = "mistral-tiny",
                messages = new[]
                {
            new { role = "system", content = "Eres un consultor organizacional experto en diagnóstico empresarial y redacción profesional." },
            new { role = "user", content = prompt }
        },
                temperature = 0.7,
                max_tokens = 2500
            };

            var json = JsonSerializer.Serialize(request);
            var message = new HttpRequestMessage(HttpMethod.Post, baseUrl);
            message.Headers.Add("Authorization", $"Bearer {apiKey}");
            message.Content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(message);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Error al llamar a Mistral ({response.StatusCode}): {responseText}");

            var mistralResponse = JsonSerializer.Deserialize<MistralResponse>(responseText);
            var rawText = mistralResponse?.choices?.FirstOrDefault()?.message?.content ?? "No se pudo generar el informe.";

            return FormatearInformeHtml(rawText, esferas);

        }

        private string FormatearInformeHtml(string texto, List<EsferaResultadoDto> esferas = null)
        {
            if (string.IsNullOrWhiteSpace(texto))
                return "<p>No se pudo generar el informe.</p>";

            texto = texto.Trim();

            // Sanitizar (evita scripts)
            texto = System.Net.WebUtility.HtmlEncode(texto);

            // Normalizar saltos
            texto = texto.Replace("\r", "").Replace("\n\n", "\n");

            // Reconvertir algunos formatos detectables (Markdown-like)
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"\*\*(.*?)\*\*", "<strong>$1</strong>");
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"_(.*?)_", "<em>$1</em>");

            // ========================
            // 🔹 Inserta íconos de semáforo por nombre de esfera (si existen)
            // ========================
            if (esferas != null && esferas.Any())
            {
                foreach (var e in esferas)
                {
                    string colorIcono;
                    string emoji;

                    if (e.ColorSemaforo == SemaforoColor.Rojo)
                    {
                        colorIcono = "#dc3545";
                        emoji = "🔴";
                    }
                    else if (e.ColorSemaforo == SemaforoColor.Amarillo)
                    {
                        colorIcono = "#ffc107";
                        emoji = "🟡";
                    }
                    else
                    {
                        colorIcono = "#28a745";
                        emoji = "🟢";
                    }

                    var regex = new System.Text.RegularExpressions.Regex($@"\b{System.Text.RegularExpressions.Regex.Escape(e.Nombre)}\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    texto = regex.Replace(texto, $"<span style='color:{colorIcono}; font-weight:600'>{emoji} {e.Nombre}</span>");
                }
            }

            // ========================
            // 🔹 Estructura visual y secciones
            // ========================
            texto = System.Text.RegularExpressions.Regex.Replace(
                texto,
                @"(?m)^\s*1\.\s*(.*)",
                "<h4 class='informe-section text-primary mt-4 mb-3'><i class='bi bi-hospital'></i> $1</h4>"
            );
            texto = System.Text.RegularExpressions.Regex.Replace(
                texto,
                @"(?m)^\s*2\.\s*(.*)",
                "<h4 class='informe-section text-success mt-4 mb-3'><i class='bi bi-graph-up-arrow'></i> $1</h4>"
            );
            texto = System.Text.RegularExpressions.Regex.Replace(
                texto,
                @"(?m)^\s*3\.\s*(.*)",
                "<h4 class='informe-section text-danger mt-4 mb-3'><i class='bi bi-exclamation-triangle'></i> $1</h4>"
            );

            // Subtítulos y numeraciones internas
            texto = System.Text.RegularExpressions.Regex.Replace(
                texto,
                @"(?m)^####\s*(.*)",
                "<h6 class='fw-bold text-secondary mt-3'>$1</h6>"
            );

            // Listas (líneas que comienzan con * o -)
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"(?:^|\n)[\*\-]\s*(.*)", "<li>$1</li>");
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"(<li>.*?</li>)+", "<ul class='mb-3 ms-4'>$0</ul>");

            // Párrafos
            texto = System.Text.RegularExpressions.Regex.Replace(texto, @"(?<!</h4>)(?<!</h6>)(?<!</li>)(?<!</ul>)(?<=\n)([^<\n].+)", "<p class='mb-2'>$1</p>");

            // ========================
            // 🔹 Envolvemos todo
            // ========================
            var html = $@"
        <div class='informe-container p-3'>
            {texto}
        </div>

        <style>
            .informe-container {{
                font-family: 'Segoe UI', sans-serif;
                line-height: 1.6;
                color: #333;
                background-color: #fff;
                border-radius: 8px;
                box-shadow: 0 2px 10px rgba(0,0,0,0.05);
                padding: 2rem;
            }}
            .informe-container h4 {{
                font-weight: 600;
                border-bottom: 2px solid #eee;
                padding-bottom: .4rem;
            }}
            .informe-container h6 {{
                color: #555;
            }}
            .informe-container ul {{
                list-style-type: disc;
            }}
            .informe-container strong {{
                color: #000;
            }}
            .informe-container i {{
                margin-right: 8px;
            }}
        </style>
    ";

            return html;
        }

        public async Task<string> GenerarInformeConCacheAsync(int encuestaId, Guid token, List<EsferaResultadoDto> esferas, double promedioGeneral)
        {
            // 🔎 Buscar informe existente
            var informeExistente = await _context.InformesGenerados
                .FirstOrDefaultAsync(i => i.EncuestaId == encuestaId && i.Token == token);

            if (informeExistente != null)
            {
                // Si no está vencido, devolvemos el guardado
                if (!informeExistente.FechaExpiracion.HasValue || informeExistente.FechaExpiracion > DateTime.UtcNow)
                    return informeExistente.ContenidoHtml;
            }

            // 🚀 Generar nuevo informe
            var nuevoInformeHtml = await GenerarInformeAsync(esferas, promedioGeneral);

            // Guardar en DB
            var nuevo = new InformeGenerado
            {
                EncuestaId = encuestaId,
                Token = token,
                ContenidoHtml = nuevoInformeHtml,
                FechaGeneracion = DateTime.UtcNow,
                FechaExpiracion = DateTime.UtcNow.AddDays(30) // opcional
            };

            if (informeExistente != null)
                _context.InformesGenerados.Remove(informeExistente); // reemplazar anterior

            _context.InformesGenerados.Add(nuevo);
            await _context.SaveChangesAsync();

            return nuevoInformeHtml;
        }


        private string ConstruirPrompt(List<EsferaResultadoDto> esferas, double promedioGeneral)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Genera un informe de diagnóstico organizacional profesional en español basado en el modelo 'Esferas de Efectividad'.");
            sb.AppendLine("Los resultados van de 1 a 5. Si el promedio general es menor a 3.25, es preocupante.");
            sb.AppendLine("Si el promedio es mayor o igual a 3.75, representa una fortaleza.");
            sb.AppendLine("Redacta un informe con las siguientes secciones:");
            sb.AppendLine("1. Diagnóstico general (basado en el promedio general).");
            sb.AppendLine("2. Fortalezas (categorías con promedio >= 3.75) con explicación y cómo aprovecharlas.");
            sb.AppendLine("3. Áreas de oportunidad (categorías < 3.25) con causa probable y tres acciones concretas.");
            sb.AppendLine();
            sb.AppendLine($"Promedio general: {promedioGeneral:F2}");
            sb.AppendLine("Resultados por esfera:");
            foreach (var e in esferas)
            {
                sb.AppendLine($"- {e.Nombre}: {e.Promedio:F2}");
            }
            sb.AppendLine();
            sb.AppendLine("Redacta el informe con títulos y subtítulos, estilo profesional, claro y persuasivo.");
            return sb.ToString();
        }

        private class MistralResponse
        {
            public List<Choice> choices { get; set; }
        }

        private class Choice
        {
            public Message message { get; set; }
        }

        private class Message
        {
            public string content { get; set; }
        }
    }
}
