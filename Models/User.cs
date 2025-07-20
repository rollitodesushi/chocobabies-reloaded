using Microsoft.AspNetCore.Identity;

namespace ChocobabiesReloaded.Models
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; } 

    }
}
