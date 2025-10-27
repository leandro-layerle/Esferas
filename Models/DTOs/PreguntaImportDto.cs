namespace Esferas.Models.DTOs
{
    public class PreguntaImportDto
    {
        public string Texto { get; set; }
        public int TipoRespuesta { get; set; } // 0 = Escala, 1 = EscalaYJustificacion, 2 = TextoLibre
        public string CategoriaPrimaria { get; set; }
        public string CategoriaSecundaria { get; set; }
        public string CategoriaTerciaria { get; set; }
        public bool EsPersonalizable { get; set; }
        public bool EsRelevanteParaSemaforo { get; set; }
    }
}
