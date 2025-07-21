using System.Net.Sockets;

namespace ChocobabiesReloaded.Models
{
    public class Participante
    {

        public int Id { get; set; }
        public int? userId { get; set; } // Nullable to allow participants without a User
        public User? user { get; set; } // Nullable navigation property
        public string nombre { get; set; } // Full name of the participant
        public string email { get; set; } // Email of the participant
        public string numeroTelefonico { get; set; } // Phone number of the participant
        
    }
}
