using System;

namespace Esferas.Models.Entities
{
    public class InformeGenerado
    {
        public int Id { get; set; }

        public int EncuestaId { get; set; }
        public Encuesta Encuesta { get; set; }

        public Guid Token { get; set; }

        public string ContenidoHtml { get; set; }

        public DateTime FechaGeneracion { get; set; }

        public DateTime? FechaExpiracion { get; set; }
    }
}

