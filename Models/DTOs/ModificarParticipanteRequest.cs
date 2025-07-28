namespace ChocobabiesReloaded.Models.DTOs
{
    public class ModificarParticipanteRequest
    {

        public int tiqueteId { get; set; }
        public string? nombre { get; set; }
        public string? telefono { get; set; }
        public string? email { get; set; }
        public estadoTiquete? estado { get; set; }
        public string? comentarios { get; set; }

    }
}
