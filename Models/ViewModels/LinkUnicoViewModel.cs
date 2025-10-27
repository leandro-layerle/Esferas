using System.ComponentModel.DataAnnotations;
using Esferas.Models.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Esferas.Models.ViewModels
{
    public class LinkUnicoViewModel
    {
        public Guid? Id { get; set; }

        public int EncuestaId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una fecha de expiración")]
        [Display(Name = "Fecha de expiración")]
        [DataType(DataType.Date)]
        public DateTime? FechaExpiracion { get; set; }

        [Range(1, 1000, ErrorMessage = "Debe generar al menos 1 link")]
        [Display(Name = "Cantidad de links")]
        public int Cantidad { get; set; } = 1;

        [Display(Name = "Estado del link")]
        public EstadoLink Estado { get; set; } = EstadoLink.Activo;

        [Display(Name = "Usado")]
        public bool Usado { get; set; } = false;

        [ValidateNever]
        public SelectList ListaEstados { get; set; }
    }
}
