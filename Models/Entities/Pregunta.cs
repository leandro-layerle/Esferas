using Esferas.Models.Enums;

namespace Esferas.Models.Entities
{
    public class Pregunta
    {
        public int Id { get; set; }

        public int EncuestaId { get; set; }
        public Encuesta Encuesta { get; set; }

        public string Texto { get; set; }
        public TipoRespuesta TipoRespuesta { get; set; }

        public int CategoriaPrimariaId { get; set; }
        public Categoria CategoriaPrimaria { get; set; }

        public int CategoriaSecundariaId { get; set; }
        public Categoria CategoriaSecundaria { get; set; }
        public int? CategoriaTerciariaId { get; set; }
        public Categoria CategoriaTerciaria { get; set; }

        public bool EsPersonalizable { get; set; }
        public bool EsRelevanteParaSemaforo { get; set; }

        public ICollection<Respuesta> Respuestas { get; set; }
    }
}
