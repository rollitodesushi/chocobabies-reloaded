namespace ChocobabiesReloaded.Models
{
    public class Rifa
    {

        public int Id { get; set; }
        public string nombre { get; set; }
        public DateTime fechaSorteo { get; set; }
        public bool vigente { get; set; }
        public decimal valorTiquete { get; set; }
        public List<Tiquete> tiquetes { get; set; }

    }
}
