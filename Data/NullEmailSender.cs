using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;

namespace ChocobabiesReloaded.Data
{
    public class NullEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Simula el envío de correo (no hace nada)
            return Task.CompletedTask;
        }
    }
}
