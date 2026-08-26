using Microsoft.AspNetCore.Mvc;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace AutoGestionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JustificativosController : ControllerBase
    {
        private readonly TuDbContext _context; 

        public JustificativosController(TuDbContext context)
        {
            _context = context;
        }

        [HttpPost("cargar")]
        public async Task<IActionResult> CargarJustificativo([FromForm] CargarJustificativoDto dto)
        {
            //Si no es por causas personales, el PDF es obligatorio
            bool esCausaPersonal = dto.TipoInasistencia == "Causas Personales";
            
            if (!esCausaPersonal)
            {
                if (dto.DocumentoPdf == null || dto.DocumentoPdf.ContentType != "application/pdf")
                {
                    return BadRequest(new { message = "Debes adjuntar en formato PDF" });
                }
            }

            var usuario = await _context.Usuarios.FindAsync(dto.IdUsuario);
            if (usuario == null) return NotFound(new { message = "Usuario no encontrado" });

            string? rutaGuardadoFinal = null;

            //Formatear nombre del archivo y guardarlo
            if (dto.DocumentoPdf != null)
            {
                string nombreOriginal = Path.GetFileNameWithoutExtension(dto.DocumentoPdf.FileName).Replace(" ", "_");
                string extension = Path.GetExtension(dto.DocumentoPdf.FileName); // Debería ser .pdf
                
                // Formato: ISCGB_NombreyApellido_NombreDocumento
                string nombreArchivoFinal = $"ISCGB_{usuario.Nombre}{usuario.Apellido}_Justificativo{extension}";
                
                var carpetaDestino = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "justificativos");
                if (!Directory.Exists(carpetaDestino)) Directory.CreateDirectory(carpetaDestino);
                
                string rutaFisica = Path.Combine(carpetaDestino, nombreArchivoFinal);

                using (var stream = new FileStream(rutaFisica, FileMode.Create))
                {
                    await dto.DocumentoPdf.CopyToAsync(stream);
                }

                rutaGuardadoFinal = $"/uploads/justificativos/{nombreArchivoFinal}";
            }

            var nuevoJustificativo = new Justificativo
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

            
            return Ok(new { 
                message = "Recorda llevar el día que te incorporas a tus actividades laborales el certificado de manera física" 
            });
        }

        // Endpoint para auditar justificativo
        [HttpPut("auditar/{idJustificativo}")]
        public async Task<IActionResult> AuditarJustificativo(int idJustificativo, [FromBody] AuditarJustificativoDto dto)
        {
            
            var justificativo = await _context.Justificativos.FindAsync(idJustificativo);
    
            if (justificativo == null)
            {
                return NotFound(new { message = "Justificativo no encontrado" });
            }

            var auditor = await _context.Usuarios.FindAsync(dto.IdUsuarioAuditor);
            if (auditor == null)
            {
                return BadRequest(new { message = "El usuario auditor no existe" });
            }

            
            justificativo.Estado = dto.Estado;
            justificativo.IdUsuarioAuditor = dto.IdUsuarioAuditor;
    
            // (Consultar) Si es necesario agregar una fecha de auditoría:
            // justificativo.FechaAuditoria = DateTime.Now;

            
            _context.Justificativos.Update(justificativo);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = $"Justificativo {dto.Estado.ToLower()} con éxito",
                estadoActual = justificativo.Estado
            });
        }

        //Endpoint que trae todos los justificativos pendientes.
        [HttpGet("pendientes")]
        public async Task<IActionResult> ObtenerJustificativosPendientes()
        {
            var pendientes = await _context.Justificativos
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

    }
}