using System.ComponentModel.DataAnnotations;
using Esferas.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Esferas.Models.ViewModels
{
    public class PreguntaViewModel
    {
        public int? Id { get; set; }

        public int EncuestaId { get; set; }

        [Required]
        public string Texto { get; set; }

        [Required]
        public TipoRespuesta TipoRespuesta { get; set; }

        [Display(Name = "Categoría Primaria")]
        public int CategoriaPrimariaId { get; set; }

        [Display(Name = "Categoría Secundaria")]
        public int CategoriaSecundariaId { get; set; }

        [Display(Name = "Categoría Terciaria (opcional)")]
        public int? CategoriaTerciariaId { get; set; }

        public bool EsPersonalizable { get; set; }

        public bool EsRelevanteParaSemaforo { get; set; }

        [ValidateNever]
        public SelectList CategoriasPrimarias { get; set; }

        [ValidateNever]
        public SelectList CategoriasSecundarias { get; set; }

        [ValidateNever]
        public SelectList CategoriasTerciarias { get; set; }
    }
}
