namespace ChocobabiesReloaded.Models.DTOs
{
    public class ModificarParticipanteRequest
    {

        public int tiqueteId { get; set; }
        public string? nombre { get; set; }
        public string? telefono { get; set; }
        public string? email { get; set; }
        public int? estado { get; set; } // Cambiar de estadoTiquete? a int?
        public string? comentarios { get; set; }

    }
}
