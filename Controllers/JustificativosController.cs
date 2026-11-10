using Microsoft.AspNetCore.Mvc;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using AutoGestionAPI.Seguridad;
using AutoGestionAPI.Services;
using System.Text.RegularExpressions;

namespace AutoGestionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JustificativosController : ControllerBase
    {
        private readonly TuDbContext _context;
        private readonly IEmailService _emailService;
        private readonly ILogger<JustificativosController> _logger;

        public JustificativosController(TuDbContext context, IEmailService emailService, ILogger<JustificativosController> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost("cargar")]
        public async Task<IActionResult> CargarJustificativo([FromForm] CargarJustificativoDto dto)
        {
            //Si no es por causas personales, el PDF es obligatorio
            bool esCausaPersonal = dto.TipoInasistencia == "Causas Personales";

            // Solo se pueden cargar justificativos propios (salvo Dirección/Secretaría)
            if (!User.PuedeAccederA(dto.IdUsuario))
                return Forbid();

            if (!esCausaPersonal && dto.DocumentoPdf == null)
            {
                return BadRequest(new { message = "Debes adjuntar en formato PDF" });
            }

            if (dto.DocumentoPdf != null)
            {
                string? errorArchivo = ValidadorArchivos.Validar(dto.DocumentoPdf, ValidadorArchivos.SoloPdf);
                if (errorArchivo != null)
                    return BadRequest(new { message = errorArchivo });
            }

            Usuario? usuario = await _context.Usuarios.FindAsync(dto.IdUsuario);
            if (usuario == null) return NotFound(new { message = "Usuario no encontrado" });

            string? rutaGuardadoFinal = null;

            //Formatear nombre del archivo y guardarlo
            if (dto.DocumentoPdf != null)
            {
                // Formato: ISCGB_NombreyApellido_Justificativo_fecha_código.pdf
                // El nombre se limpia (solo letras y números) para que no se pueda
                // armar una ruta rara, y el código aleatorio hace que la URL no se pueda adivinar.
                string nombreLimpio = Regex.Replace($"{usuario.Nombre}{usuario.Apellido}", @"[^\p{L}\p{N}]", "");
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string codigo = Guid.NewGuid().ToString("N")[..12];
                string nombreArchivoFinal = $"ISCGB_{nombreLimpio}_Justificativo_{timestamp}_{codigo}.pdf";

                string carpetaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "justificativos");
                if (!Directory.Exists(carpetaDestino)) Directory.CreateDirectory(carpetaDestino);

                string rutaFisica = Path.Combine(carpetaDestino, nombreArchivoFinal);

                using (FileStream stream = new FileStream(rutaFisica, FileMode.Create))
                {
                    await dto.DocumentoPdf.CopyToAsync(stream);
                }

                rutaGuardadoFinal = $"/uploads/justificativos/{nombreArchivoFinal}";
            }

            Justificativo nuevoJustificativo = new Justificativo
            {
                IdUsuario = dto.IdUsuario,
                TipoInasistencia = dto.TipoInasistencia,
                NotaAdicional = dto.NotaAdicional,
                FechaCarga = DateTime.Now,
                RutaArchivo = rutaGuardadoFinal,
                Estado = "Pendiente",
                FechaInasistenciaInicio = dto.FechaInasistenciaInicio,
                FechaInasistenciaFin = dto.FechaInasistenciaFin
            };

            _context.Justificativos.Add(nuevoJustificativo);
            await _context.SaveChangesAsync();


            return Ok(new
            {
                message = "Recorda llevar el día que te incorporas a tus actividades laborales el certificado de manera física"
            });
        }

        // Endpoint para auditar justificativo
        [HttpPut("auditar/{idJustificativo}")]
        [Authorize(Roles = UsuarioActual.RolesAdministrativos)]
        public async Task<IActionResult> AuditarJustificativo(int idJustificativo, [FromBody] AuditarJustificativoDto dto)
        {

            Justificativo? justificativo = await _context.Justificativos.FindAsync(idJustificativo);

            if (justificativo == null)
            {
                return NotFound(new { message = "Justificativo no encontrado" });
            }

            // El auditor es quien está logueado (no se confía en el id que manda el body)
            int? idAuditor = User.ObtenerIdUsuario();
            if (idAuditor == null) return Unauthorized();


            bool recienRechazado = EsRechazado(dto.Estado) && !EsRechazado(justificativo.Estado);

            justificativo.Estado = dto.Estado;
            justificativo.IdUsuarioAuditor = idAuditor.Value;

            // (Consultar) Si es necesario agregar una fecha de auditoría:
            // justificativo.FechaAuditoria = DateTime.Now;


            _context.Justificativos.Update(justificativo);
            await _context.SaveChangesAsync();

            // Si se acaba de rechazar, se le avisa por mail. Si el mail falla, la
            // auditoría ya quedó guardada igual.
            if (recienRechazado)
            {
                try
                {
                    Usuario usuario = await _context.Usuarios.FirstAsync(u => u.IdUsuario == justificativo.IdUsuario);

                    string periodo = justificativo.FechaInasistenciaInicio.HasValue
                        ? justificativo.FechaInasistenciaFin.HasValue && justificativo.FechaInasistenciaFin.Value.Date != justificativo.FechaInasistenciaInicio.Value.Date
                            ? $" del {justificativo.FechaInasistenciaInicio:dd/MM/yyyy} al {justificativo.FechaInasistenciaFin:dd/MM/yyyy}"
                            : $" del {justificativo.FechaInasistenciaInicio:dd/MM/yyyy}"
                        : "";
                    string tipo = string.IsNullOrWhiteSpace(justificativo.TipoInasistencia) ? "" : $" ({justificativo.TipoInasistencia})";

                    await _emailService.EnviarAvisoRechazoAsync(
                        usuario.Email,
                        usuario.Nombre ?? "Usuario",
                        $"tu justificativo de inasistencia{periodo}{tipo}",
                        dto.Comentario,
                        "Si corresponde, cargá uno nuevo desde el sistema o acercate a Secretaría.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "No se pudo enviar el aviso de rechazo del justificativo {IdJustificativo}.", idJustificativo);
                }
            }

            return Ok(new
            {
                message = $"Justificativo {dto.Estado.ToLower()} con éxito",
                estadoActual = justificativo.Estado
            });
        }

        private static bool EsRechazado(string? estado)
        {
            return string.Equals(estado?.Trim(), "Rechazado", StringComparison.OrdinalIgnoreCase);
        }

        //Endpoint que trae todos los justificativos pendientes.
        [HttpGet("pendientes")]
        [Authorize(Roles = UsuarioActual.RolesAdministrativos)]
        public async Task<IActionResult> ObtenerJustificativosPendientes()
        {
            List<JustificativoPendienteDto> pendientes = await _context.Justificativos
                .Where(j => j.Estado == "Pendiente")
                .Select(j => new JustificativoPendienteDto
                {
                    IdJustificativo = j.IdJustificativo,
                    // Armamos el nombre uniendo Nombre y Apellido del usuario:
                    NombreDocente = j.IdUsuarioNavigation.Nombre + " " + j.IdUsuarioNavigation.Apellido,
                    TipoInasistencia = j.TipoInasistencia,
                    RutaArchivo = j.RutaArchivo,
                    FechaCarga = j.FechaCarga
                })
                .ToListAsync();

            return Ok(pendientes);
        }

        // Endpoint que trae todos los justificativos de un usuario específico.
        [HttpGet("{idUsuario}/justificativos")]
        public async Task<IActionResult> ObtenerJustificativosDeUsuario(int idUsuario)
        {
            if (!User.PuedeAccederA(idUsuario))
                return Forbid();

            Usuario? usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario);

            if (usuario == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }


            string nombreCompleto = $"{usuario.Nombre} {usuario.Apellido}".Trim();


            List<JustificativoResponseDto> justificativos = await _context.Justificativos
                .Where(j => j.IdUsuario == idUsuario)
                .Select(j => new JustificativoResponseDto
                {
                    IdJustificativo = j.IdJustificativo,
                    TipoInasistencia = j.TipoInasistencia,
                    RutaArchivo = j.RutaArchivo,
                    NotaAdicional = j.NotaAdicional,
                    FechaCarga = j.FechaCarga,
                    Estado = j.Estado,
                    FechaInasistenciaInicio = j.FechaInasistenciaInicio,
                    FechaInasistenciaFin = j.FechaInasistenciaFin,
                    IdUsuarioAuditor = j.IdUsuarioAuditor
                })
                .OrderByDescending(j => j.FechaCarga)
                .ToListAsync();


            if (!justificativos.Any())
            {
                return Ok(new
                {
                    nombreUsuario = nombreCompleto,
                    message = "El usuario no tiene justificativos presentados.",
                    data = new List<JustificativoResponseDto>()
                });
            }


            return Ok(new
            {
                nombreUsuario = nombreCompleto,
                data = justificativos
            });
        }

        [HttpGet("todos")]
        [Authorize(Roles = UsuarioActual.RolesAdministrativos)]
        public async Task<IActionResult> ObtenerTodosLosJustificativos()
        {
            List<JustificativoGeneralDto> justificativos = await _context.Justificativos
                .Join(_context.Usuarios,
                    j => j.IdUsuario,
                    u => u.IdUsuario,
                    (j, u) => new JustificativoGeneralDto
                    {
                        IdJustificativo = j.IdJustificativo,
                        // Concatenamos el nombre y apellido del usuario
                        NombreUsuario = (u.Nombre + " " + u.Apellido).Trim(),
                        TipoInasistencia = j.TipoInasistencia,
                        Estado = j.Estado,
                        FechaCarga = j.FechaCarga,
                        FechaInasistenciaInicio = j.FechaInasistenciaInicio,
                        FechaInasistenciaFin = j.FechaInasistenciaFin
                    })
                .OrderByDescending(j => j.FechaCarga)
                .ToListAsync();

            if (!justificativos.Any())
            {
                return Ok(new { message = "No hay justificativos cargados en el sistema.", data = new List<JustificativoGeneralDto>() });
            }

            return Ok(new { data = justificativos });
        }

    }
}