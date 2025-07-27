using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace ChocobabiesReloaded.Models
{
    public class Rifa
    {

        public int id { get; set; }

        [Required]
        public string nombreSorteo { get; set; }
       
        public DateTime fechaCreacion { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.Date)]
        [CustomValidation(typeof(Rifa), nameof(ValidarFechaCierre))]
        public DateTime fechaCierreSorteo { get; set; }

        

        [Required]
        [DataType(DataType.Currency)]
        public decimal precioPorNumero { get; set; }
        public bool vigente { get; set; }
        

        // *** Added: CantidadNumeros for ticket count ***

        [Required]
        public int cantidadNumeros { get; set; }

        [BindNever]
        public ICollection<Tiquete> tiquetes { get; set; }

        public static ValidationResult ValidarFechaCierre(DateTime fecha, ValidationContext context)
        {
            return fecha.Date < DateTime.Now.Date
                ? new ValidationResult("La fecha de cierre debe ser hoy o una fecha futura.")
                : ValidationResult.Success;
        }


    }

}
