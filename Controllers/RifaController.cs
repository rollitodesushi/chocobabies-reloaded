using ChocobabiesReloaded.Data;
using ChocobabiesReloaded.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize(Roles = "Admin")]
public class RifaController : Controller
{
    private readonly RifaDbContext _context;
    private readonly UserManager<User> _userManager;

    public RifaController(RifaDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public IActionResult Crear()
    {
        return View(new Rifa { cantidadNumeros = 100 });
    }

    [HttpPost]
    public async Task<IActionResult> Crear(Rifa rifa)
    {
        if (ModelState.IsValid)
        {
            rifa.vigente = true; // Set default value
            _context.rifas.Add(rifa);
            await _context.SaveChangesAsync();

            // Generar tiquetes según cantidadNumeros
            var tiquetes = Enumerable.Range(0, rifa.cantidadNumeros).Select(n => new Tiquete
            {
                rifaID = rifa.id,
                numeroTiquete = n,
                estaComprado = false,
                fechaCompra = DateTime.MinValue
            }).ToList();
            _context.tiquetes.AddRange(tiquetes);
            await _context.SaveChangesAsync();

            return RedirectToAction("VerRifa", new { id = rifa.id });
        }
        return View(rifa);
    }

    public async Task<IActionResult> VerRifa(int id)
    {
        var rifa = await _context.rifas
            .Include(r => r.tiquetes)
            .FirstOrDefaultAsync(r => r.id == id);
        if (rifa == null) return NotFound();
        return View(rifa);
    }

    [HttpPost]
    public async Task<IActionResult> AsignarNumero(int rifaID, int numeroTiquete, string participanteEmail, string nombre = null, string telefono = null)
    {
        var rifa = await _context.rifas.Include(r => r.tiquetes)
            .FirstOrDefaultAsync(r => r.id == rifaID);
        if (rifa == null) return NotFound();

        var tiquete = rifa.tiquetes.FirstOrDefault(t => t.numeroTiquete == numeroTiquete);
        if (tiquete == null || tiquete.estaComprado) return Json(new { success = false });

        Participante participante = null;
        if (!string.IsNullOrEmpty(participanteEmail))
        {
            // Buscar primero por email del participante
            participante = await _context.participantes
                .FirstOrDefaultAsync(p => p.email == participanteEmail);

            if (participante == null && participanteEmail != null)
            {
                // Buscar participantes con user y comparar email asíncronamente
                var participantesConUser = await _context.participantes
                    .Where(p => p.user != null)
                    .ToListAsync();
                foreach (var p in participantesConUser)
                {
                    var userEmail = await _userManager.GetEmailAsync(p.user);
                    if (userEmail == participanteEmail)
                    {
                        participante = p;
                        break;
                    }
                }
            }
        }

        if (participante == null && (!string.IsNullOrEmpty(nombre) || !string.IsNullOrEmpty(telefono)))
        {
            // Buscar o crear participante por nombre y/o teléfono
            participante = await _context.participantes
                .FirstOrDefaultAsync(p => (string.IsNullOrEmpty(nombre) || p.nombre == nombre) && (string.IsNullOrEmpty(telefono) || p.numeroTelefonico == telefono));
            if (participante == null)
            {
                participante = new Participante
                {
                    nombre = nombre ?? "Participante Anónimo",
                    numeroTelefonico = telefono ?? "",
                    email = participanteEmail // Usar email si se proporcionó
                };
                _context.participantes.Add(participante);
                await _context.SaveChangesAsync();
            }
        }

        if (participante == null)
        {
            return Json(new { success = false, message = "Debe proporcionar un email, nombre o teléfono válidos." });
        }

        tiquete.participanteId = participante.id;
        tiquete.estaComprado = true;
        tiquete.fechaCompra = DateTime.Now;
        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }

    public async Task<IActionResult> Lista()
    {
        var rifas = await _context.rifas.ToListAsync();
        return View(rifas);
    }
}
