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

        // Zona horaria de Costa Rica
        var zonaCR = TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");

        // Fecha actual en Costa Rica
        var ahoraCR = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, zonaCR);

        // Validar que la fecha de cierre no sea en el pasado (respecto a CR)
        var fechaCierreLocal = DateTime.SpecifyKind(rifa.fechaCierreSorteo, DateTimeKind.Unspecified);
        if (fechaCierreLocal < ahoraCR)
        {
            ModelState.AddModelError("fechaCierreSorteo", "La fecha de cierre no puede ser anterior a la fecha actual en Costa Rica.");
            return View(rifa);
        }

        if (!ModelState.IsValid)
        {
            var allErrors = ModelState.Values.SelectMany(v => v.Errors);
            foreach (var error in allErrors)
            {
                Console.WriteLine("❌ ERROR: " + error.ErrorMessage);
            }
            return View(rifa);
        }

        // Convertir ambas fechas a UTC antes de guardar
        rifa.fechaCreacion = DateTime.UtcNow;
        rifa.fechaCierreSorteo = TimeZoneInfo.ConvertTimeToUtc(fechaCierreLocal, zonaCR);
        rifa.vigente = true;

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
             .ThenInclude(t => t.participante)
         .FirstOrDefaultAsync(r => r.id == id);

        if (rifa == null) return NotFound();
        return View(rifa);
    }

    
    [HttpPost]
    public async Task<IActionResult> AsignarNumero([FromBody] AsignarNumeroRequest request)
    {
        var rifa = await _context.rifas
            .Include(r => r.tiquetes)
            .FirstOrDefaultAsync(r => r.id == request.rifaID);

        if (rifa == null)
            return NotFound();

        var tiquete = rifa.tiquetes.FirstOrDefault(t => t.numeroTiquete == request.numeroTiquete);
        if (tiquete == null || tiquete.estaComprado)
            return Json(new { success = false, message = "El número ya está asignado o no existe." });

        // Validación obligatoria
        if (string.IsNullOrWhiteSpace(request.nombre) || string.IsNullOrWhiteSpace(request.telefono))
        {
            return Json(new
            {
                success = false,
                message = "Debe proporcionar al menos nombre y teléfono del participante."
            });
        }

        Participante participante = null;

        // Si hay email, buscar por email primero
        if (!string.IsNullOrWhiteSpace(request.participanteEmail))
        {
            participante = await _context.participantes
                .FirstOrDefaultAsync(p => p.email == request.participanteEmail);

            if (participante == null)
            {
                // También verificar si hay usuario relacionado con ese correo
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

        // Si no se encontró, buscar por nombre y teléfono
        if (participante == null)
        {
            participante = await _context.participantes.FirstOrDefaultAsync(p =>
                p.nombre == request.nombre &&
                p.numeroTelefonico == request.telefono
            );
        }

        // Si no existe todavía, lo creamos
        if (participante == null)
        {
            participante = new Participante
            {
                nombre = request.nombre,
                numeroTelefonico = request.telefono,
                email = request.participanteEmail
            };
            _context.participantes.Add(participante);
            await _context.SaveChangesAsync();
        }

        // Asignar tiquete
        // Determinar estado real del tiquete
        var estadoFinal = estadoTiquete.Disponible; // valor por defecto

        if (request.estado.HasValue && Enum.IsDefined(typeof(estadoTiquete), request.estado.Value))
        {
            estadoFinal = (estadoTiquete)request.estado.Value;
        }
        else
        {
            estadoFinal = estadoTiquete.Comprado; // o el que desees por defecto
        }


        // Validación: si hay un participante, el estado NO puede ser Disponible
        if (estadoFinal == estadoTiquete.Disponible && participante != null)
        {
            return Json(new
            {
                success = false,
                message = "No se puede asignar un participante si el estado es Disponible."
            });
        }

        tiquete.participanteId = participante?.id;
        tiquete.estado = estadoFinal;
        tiquete.comentarios = request.comentarios;
        tiquete.estaComprado = estadoFinal == estadoTiquete.Comprado;
        tiquete.fechaCompra = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }

    [HttpPost]
    public async Task<IActionResult> ModificarParticipante([FromBody] ModificarParticipanteRequest request)
    {
        var tiquete = await _context.tiquetes
            .Include(t => t.participante)
            .FirstOrDefaultAsync(t => t.id == request.tiqueteId);

        if (tiquete == null)
            return Json(new { success = false, message = "Tiquete no encontrado." });

        // Validación obligatoria
        if (string.IsNullOrWhiteSpace(request.nombre) || string.IsNullOrWhiteSpace(request.telefono))
        {
            return Json(new
            {
                success = false,
                message = "Debe proporcionar al menos nombre y teléfono del participante."
            });
        }

        var participante = tiquete.participante;

        if (participante == null)
        {
            participante = new Participante();
            _context.participantes.Add(participante);
        }

        participante.nombre = request.nombre;
        participante.numeroTelefonico = request.telefono;
        participante.email = request.email;

        // Actualizar solo si se proporcionó nuevo comentario
        if (!string.IsNullOrWhiteSpace(request.comentarios))
        {
            tiquete.comentarios = request.comentarios;
        }

        // Actualizar estado si fue enviado (por ejemplo, "Reservado")
        if (request.estado.HasValue && Enum.IsDefined(typeof(estadoTiquete), request.estado.Value))
        {
            tiquete.estado = (estadoTiquete)request.estado.Value;
        }


        await _context.SaveChangesAsync();
        return Json(new { success = true });
    }


    public async Task<IActionResult> Lista()
    {
        var rifas = await _context.rifas.ToListAsync();
        return View(rifas);
    }
}
