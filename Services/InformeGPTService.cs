using Esferas.Models.DTOs;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Esferas.Services
{
    public class InformeGPTService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;

        public InformeGPTService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _config = config;
        }

        public async Task<string> GenerarInformeAsync(List<EsferaResultadoDto> esferas, double promedioGeneral)
        {
            var apiKey = _config["OpenAI:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("API key de OpenAI no configurada.");

            var prompt = ConstruirPrompt(esferas, promedioGeneral);

            var request = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
                    new { role = "system", content = "Eres un consultor organizacional experto en diagnóstico empresarial y redacción profesional." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7
            };

            var requestJson = JsonSerializer.Serialize(request);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
            };
            httpRequest.Headers.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<OpenAIResponse>(responseJson);

            return result?.Choices?[0]?.Message?.Content ?? "No se pudo generar el informe.";
        }

        private string ConstruirPrompt(List<EsferaResultadoDto> esferas, double promedioGeneral)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Quiero que redactes un informe de diagnóstico organizacional en español, basado en una herramienta llamada 'Esferas de Efectividad'.");
            sb.AppendLine("Los resultados de la encuesta van de 1 a 5. Si el promedio general está por debajo de 3.25 es preocupante.");
            sb.AppendLine("Esferas con puntaje mayor o igual a 3.75 son fortalezas. Esferas con puntaje menor a 3.25 son áreas de oportunidad.");
            sb.AppendLine();

            sb.AppendLine($"Promedio general: {promedioGeneral:F2}");
            sb.AppendLine("Resultados por esfera:");
            foreach (var e in esferas)
            {
                sb.AppendLine($"- {e.Nombre}: {e.Promedio:F2}");
            }

            sb.AppendLine();
            sb.AppendLine("Con esta información, quiero que redactes lo siguiente:");
            sb.AppendLine("1. Un párrafo introductorio de diagnóstico general.");
            sb.AppendLine("2. Una sección de Fortalezas. Incluye cada esfera con puntaje >= 3.75, explica qué implica ese puntaje, y cómo la organización puede apalancar esa fortaleza.");
            sb.AppendLine("3. Una sección de Áreas de Oportunidad. Para cada esfera < 3.25, incluye:");
            sb.AppendLine("   - Una causa raíz probable (breve análisis de por qué puede estar baja).");
            sb.AppendLine("   - 3 acciones concretas y específicas que se pueden tomar para mejorar.");
            sb.AppendLine();
            sb.AppendLine("Quiero un texto claro, profesional, con títulos y subtítulos. Escribe como consultor experto.");

            return sb.ToString();
        }

        private class OpenAIResponse
        {
            [JsonPropertyName("choices")]
            public List<Choice> Choices { get; set; }
        }

        private class Choice
        {
            [JsonPropertyName("message")]
            public Message Message { get; set; }
        }

        private class Message
        {
            [JsonPropertyName("content")]
            public string Content { get; set; }
        }
    }
}
