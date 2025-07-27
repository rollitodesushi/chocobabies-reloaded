using ChocobabiesReloaded.Data;
using ChocobabiesReloaded.Models;
using ChocobabiesReloaded.Models.DTOs;
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

        ModelState.Remove("tiquetes");
        if (!ModelState.IsValid)
        {
            // Mostramos errores por consola para depurar
            var allErrors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in allErrors)
            {
                Console.WriteLine("❌ ERROR: " + error.ErrorMessage);
            }
            return View(rifa);
        }

        rifa.vigente = true;
        rifa.fechaCierreSorteo = DateTime.SpecifyKind(rifa.fechaCierreSorteo, DateTimeKind.Utc);
        _context.rifas.Add(rifa);
        await _context.SaveChangesAsync();

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

    public async Task<IActionResult> VerRifa(int id)
    {
        var rifa = await _context.rifas
            .Include(r => r.tiquetes)
            .FirstOrDefaultAsync(r => r.id == id);
        if (rifa == null) return NotFound();
        return View(rifa);
    }

    [HttpPost]
    public async Task<IActionResult> AsignarNumero([FromBody] AsignarNumeroRequest request)
    {
        var rifa = await _context.rifas.Include(r => r.tiquetes)
            .FirstOrDefaultAsync(r => r.id == request.rifaID);
        if (rifa == null) return NotFound();

        var tiquete = rifa.tiquetes.FirstOrDefault(t => t.numeroTiquete == request.numeroTiquete);
        if (tiquete == null || tiquete.estaComprado) return Json(new { success = false });

        Participante participante = null;

        if (!string.IsNullOrEmpty(request.participanteEmail))
        {
            participante = await _context.participantes
                .FirstOrDefaultAsync(p => p.email == request.participanteEmail);

            if (participante == null)
            {
                var participantesConUser = await _context.participantes
                    .Where(p => p.user != null)
                    .ToListAsync();

                foreach (var p in participantesConUser)
                {
                    var userEmail = await _userManager.GetEmailAsync(p.user);
                    if (userEmail == request.participanteEmail)
                    {
                        participante = p;
                        break;
                    }
                }
            }
        }

        if (participante == null && (!string.IsNullOrEmpty(request.nombre) || !string.IsNullOrEmpty(request.telefono)))
        {
            participante = await _context.participantes
                .FirstOrDefaultAsync(p =>
                    (string.IsNullOrEmpty(request.nombre) || p.nombre == request.nombre) &&
                    (string.IsNullOrEmpty(request.telefono) || p.numeroTelefonico == request.telefono));

            if (participante == null)
            {
                participante = new Participante
                {
                    nombre = request.nombre ?? "Participante Anónimo",
                    numeroTelefonico = request.telefono ?? "",
                    email = request.participanteEmail
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
        tiquete.fechaCompra = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }

    public async Task<IActionResult> Lista()
    {
        var rifas = await _context.rifas.ToListAsync();
        return View(rifas);
    }
}
