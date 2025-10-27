using System;
namespace Esferas.Models.Entities
{
    public class Respuesta
    {
        public int Id { get; set; }

        public int PreguntaId { get; set; }
        public Pregunta Pregunta { get; set; }

        public int EncuestaId { get; set; }
        public Encuesta Encuesta { get; set; }

        public Guid LinkUnicoId { get; set; }
        public LinkUnico LinkUnico { get; set; }

        public int? RespuestaNumerica { get; set; }
        public string? Justificacion { get; set; }
        public string? RespuestaTexto { get; set; }

        public DateTime Fecha { get; set; }
    }
}