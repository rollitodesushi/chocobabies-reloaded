using System.ComponentModel.DataAnnotations;

namespace ChocobabiesReloaded.Models
{
    public class Rifa
    {

        public int id { get; set; }

        [Required]
        public string nombreSorteo { get; set; }
        public DateTime fechaCreacion { get; set; } = DateTime.Now;

        [Required]
        [DataType(DataType.Date)]
        public DateTime fechaCierreSorteo { get; set; }

        

        [Required]
        [DataType(DataType.Currency)]
        public decimal precioPorNumero { get; set; }
        public bool vigente { get; set; }
        

        // *** Added: CantidadNumeros for ticket count ***

        [Required]
        public int cantidadNumeros { get; set; }

        public ICollection<Tiquete> tiquetes { get; set; }


    }

}
