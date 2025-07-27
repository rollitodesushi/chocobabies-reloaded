

namespace ChocobabiesReloaded.Models
{
    public class Tiquete
    {

        public int id { get; set; }
        public int rifaID { get; set; }
        public Rifa rifa { get; set; }

        public int? participanteId { get; set; }

        public Participante participante { get; set; }

        public int numeroTiquete { get; set; }

        public DateTime fechaCompra { get; set; }

        public bool estaComprado { get; set; }

    }
}
