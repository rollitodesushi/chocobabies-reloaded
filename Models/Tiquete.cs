

namespace ChocobabiesReloaded.Models
{
    public class Tiquete
    {

        public int Id { get; set; }
        public int rifaID { get; set; }
        public Rifa rifa { get; set; }

        public int participanteID { get; set; }

        public Participante participante { get; set; }

        public int numeroTiquete { get; set; }

        public DateTime fechaCompra { get; set; }



    }
}
