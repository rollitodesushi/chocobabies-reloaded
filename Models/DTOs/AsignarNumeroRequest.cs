namespace ChocobabiesReloaded.Models.DTOs
{
    public class AsignarNumeroRequest
    {
        public int rifaID { get; set; }
        public int numeroTiquete { get; set; }
        public string participanteEmail { get; set; }
        public string nombre { get; set; }
        public string telefono { get; set; }

        public estadoTiquete? estado { get; set; }
        public string? comentarios { get; set; }
    }
}
