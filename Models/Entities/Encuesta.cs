using Esferas.Models.Enums;

namespace Esferas.Models.Entities
{
    public class Encuesta
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public Empresa Empresa { get; set; }

        public DateTime FechaCreacion { get; set; }
        public EstadoEncuesta Estado { get; set; }

        public ICollection<Pregunta> Preguntas { get; set; } = new List<Pregunta>();
        public ICollection<LinkUnico> Links { get; set; }
    }
}