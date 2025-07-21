using ChocobabiesReloaded.Data;
using ChocobabiesReloaded.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;

namespace ChocobabiesReloaded.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly RifaDbContext _context;

        public AdminController(UserManager<User> userManager, RifaDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult AssignTicket(int rifaID)
        {
            ViewBag.RifaID = rifaID;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AssignTicket(int rifaID, string nombre, string email, string numeroTelefonico, int numeroTiquete)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            var participante = new Participante
            {
                nombre = nombre,
                email = email,
                numeroTelefonico = numeroTelefonico
            };

            _context.participantes.Add(participante);
            await _context.SaveChangesAsync();

            var tiquete = new Tiquete
            {
                rifaID = rifaID,
                participanteID = participante.Id,
                numeroTiquete = numeroTiquete
            };

            _context.tiquetes.Add(tiquete);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}
