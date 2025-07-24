using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ChocobabiesReloaded.Data;
using ChocobabiesReloaded.Models;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

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

        public async Task<IActionResult> AssignTicket(int rifaID)
        {
            var rifa = await _context.rifas
                .FirstOrDefaultAsync(r => r.id == rifaID && r.vigente);
            if (rifa == null)
            {
                return NotFound("Rifa no encontrada o no está activa.");
            }

            ViewBag.RifaID = rifaID;
            ViewBag.PrecioTiquete = rifa.precioPorNumero.ToString("C", CultureInfo.GetCultureInfo("es-CO"));
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AssignTicket(int rifaID, string nombre, string email, string numeroTelefonico, int numeroTiquete)
        {
            var rifa = await _context.rifas
                    .FirstOrDefaultAsync(r => r.id == rifaID && r.vigente);
            if (!ModelState.IsValid)
            {
                
                if (rifa == null)
                {
                    return NotFound("Rifa no encontrada o no está activa.");
                }
                ViewBag.RifaID = rifaID;
                ViewBag.PrecioTiquete = rifa.precioPorNumero.ToString("C", CultureInfo.GetCultureInfo("es-CO"));
                return View();
            }

           
            if (rifa == null)
            {
                ModelState.AddModelError("", "Rifa no encontrada o no está activa.");
                ViewBag.RifaID = rifaID;
                ViewBag.PrecioTiquete = null;
                return View();
            }

            if (await _context.tiquetes.AnyAsync(t => t.rifaID == rifaID && t.numeroTiquete == numeroTiquete))
            {
                ModelState.AddModelError("", "El número de tiquete ya está asignado.");
                ViewBag.RifaID = rifaID;
                ViewBag.PrecioTiquete = rifa.precioPorNumero.ToString("C", CultureInfo.GetCultureInfo("es-CO"));
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
                participanteId = participante.id,
                numeroTiquete = numeroTiquete
            };

            _context.tiquetes.Add(tiquete);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }
    }
}