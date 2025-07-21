using ChocobabiesReloaded.Data;
using ChocobabiesReloaded.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            // Verificar si la rifa existe y es activa
            var rifa = await _context.rifas
                .FirstOrDefaultAsync(r => r.Id == rifaID && r.vigente);
            if (rifa == null)
            {
                ModelState.AddModelError("", "Rifa no encontrada o ya cerró el sorteo.");
                return View();
            }

            // Verificar si el número de tiquete ya está asignado
            if (await _context.tiquetes.AnyAsync(t => t.rifaID == rifaID && t.numeroTiquete == numeroTiquete))
            {
                ModelState.AddModelError("", "Ese numero ya fue asignado a otro participante");
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

            // Pasar el precio del tiquete a la vista
            ViewBag.PrecioTiquete = rifa.valorTiquete;
            return RedirectToAction("Index", "Home");
        }
    }
}
