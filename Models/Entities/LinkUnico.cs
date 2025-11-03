using Esferas.Models.Enums;
using System;

namespace Esferas.Models.Entities
{
    public class LinkUnico
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public int EncuestaId { get; set; }
        public Encuesta Encuesta { get; set; }

        public EstadoLink Estado { get; set; } = EstadoLink.Activo;

        public Guid Token { get; set; }

        public DateTime? FechaFinalizacion { get; set; }
        public DateTime? FechaExpiracion { get; set; }
        public DateTime? FechaCreacion { get; set; }

        public bool Usado { get; set; }
        public bool EsLinkEmpresa { get; set; } = false;
    }
}