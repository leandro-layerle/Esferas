using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Esferas.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace Esferas.Models.ViewModels
{
    public class CategoriaCreateEditViewModel
    {
        public int? Id { get; set; } // Solo usado para Edit

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [Display(Name = "Nombre")]
        public string Nombre { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        [Display(Name = "Tipo")]
        public TipoCategoria Tipo { get; set; }

        [Display(Name = "Categoría padre")]
        public int? CategoriaPadreId { get; set; }

        [ValidateNever]
        public SelectList ListaCategorias { get; set; }
    }
}
