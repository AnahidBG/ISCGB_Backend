using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;
using Microsoft.AspNetCore.Authorization; // Si utiliza JWT
using System.Security.Claims;

namespace AutoGestionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionCalendarioController : ControllerBase
    {
        private readonly TuDbContext _context;

        public ConfiguracionCalendarioController(TuDbContext context)
        {
            _context = context;
        }

        // Si usa JWT, puede descomentar la siguiente línea para restringir el acceso:
        // [Authorize(Roles = "Director, Secretario")]
        [HttpPut("limite-parciales")]
        public async Task<IActionResult> ActualizarLimiteParciales([FromBody] ActualizarLimiteParcialesDto dto)
        {
            // 1. Validación básica: un límite no puede ser negativo
            if (dto.NuevoLimite <= 0)
            {
                return BadRequest(new { message = "El límite de parciales Diarios debe ser mayor a cero." });
            }

            // 2. Buscamos el registro de configuración (asumimos que siempre hay uno principal)
            var config = await _context.ConfiguracionesSistema.FirstOrDefaultAsync();

            if (config == null)
            {
                // Si por alguna razón la tabla estuviera vacía, creamos el primer registro
                config = new ConfiguracionSistema
                {
                    LimiteParcialesDiario = dto.NuevoLimite,
                    // Si tiene otras propiedades obligatorias (como FrecuenciaNotificacionDias), agréguelas aquí.
                };
                _context.ConfiguracionesSistema.Add(config);
            }
            else
            {
                // 3. Si existe, simplemente actualizamos el valor
                config.LimiteParcialesDiario = dto.NuevoLimite;
            }

            // 4. Guardamos los cambios en la base de datos
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "El límite de parciales por día se ha actualizado con éxito.",
                nuevoLimite = config.LimiteParcialesDiario
            });
        }
        private async Task<bool> SuperaLimiteDiarioAsync(DateTime fechaDeseada, int? idExamenIgnorar = null)
        {
            // 1. Obtener el límite configurado (por defecto 2)
            var config = await _context.ConfiguracionesSistema.FirstOrDefaultAsync();
            int limite = config?.LimiteParcialesDiario ?? 2;

            // 2. Contar cuántos exámenes YA existen en esa fecha EXACTA (ignorando la hora)
            var query = _context.Examenes
                .Where(e => e.Fecha.Date == fechaDeseada.Date);

            // Si estamos editando, ignoramos el examen actual
            if (idExamenIgnorar.HasValue)
            {
                query = query.Where(e => e.IdExamen != idExamenIgnorar.Value);
            }

            int examenesEnElDia = await query.CountAsync();

            // 3. Retornar si agregar uno más superaría el límite diario
            return (examenesEnElDia + 1) > limite;
        }

        [HttpPost("registrar")]
        public async Task<IActionResult> RegistrarExamen([FromBody] CargaExamenDto dto)
        {
            bool superaLimite = await SuperaLimiteDiarioAsync(dto.Fecha);

            if (superaLimite)
            {
                return BadRequest(new { message = "No se puede registrar. Se ha superado el límite de parciales para ese día." });
            }

            var nuevoExamen = new Examene
            {
                Fecha = dto.Fecha,
                IdComision = dto.IdComision,
                IdMateria = dto.IdMateria,
                IdTipoExamen = dto.IdTipoExamen
            };

            _context.Examenes.Add(nuevoExamen);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Examen registrado correctamente." });
        }

        [HttpGet("estado-mes")]
        public async Task<IActionResult> ObtenerEstadoMes(int anio, int mes)
        {
            var config = await _context.ConfiguracionesSistema.FirstOrDefaultAsync();
            int limite = config?.LimiteParcialesDiario ?? 2;

            var fechaInicioMes = new DateTime(anio, mes, 1);
            var fechaFinMes = fechaInicioMes.AddMonths(1).AddSeconds(-1);

            var examenesDelMes = await _context.Examenes
                .Where(e => e.Fecha >= fechaInicioMes && e.Fecha <= fechaFinMes)
                .ToListAsync();

            // Agrupamos por Día exacto
            var estadoPorDia = examenesDelMes
                .GroupBy(e => e.Fecha.Date)
                .Select(grupo => new
                {
                    Fecha = grupo.Key,
                    CantidadExamenes = grupo.Count(),
                    LimiteAlcanzado = grupo.Count() >= limite
                });

            return Ok(new
            {
                limiteDiario = limite,
                diasOcupados = estadoPorDia
            });
        }
        // --- 1. ENDPOINT PARA MODIFICAR ---
        [HttpPut("modificar/{idExamen}")]
        public async Task<IActionResult> ModificarExamen(int idExamen, [FromBody] ModificarFechaExamenDto dto)
        {
            var examen = await _context.Examenes.FindAsync(idExamen);
            if (examen == null)
            {
                return NotFound(new { message = "No se encontró el examen especificado." });
            }

            // Validamos la regla de negocio, pasándole el ID del examen actual 
            // para que NO se cuente a sí mismo en el cálculo de la semana.
            bool superaLimite = await SuperaLimiteDiarioAsync(dto.NuevaFecha, idExamen);

            if (superaLimite)
            {
                return BadRequest(new { message = "No se puede reprogramar. El día ha superado el límite de parciales." });
            }

            // Si pasa la validación, actualizamos la fecha
            examen.Fecha = dto.NuevaFecha;
            await _context.SaveChangesAsync();

            return Ok(new { message = "La fecha del examen se ha modificado correctamente." });
        }


        // --- 2. ENDPOINT PARA ELIMINAR ---
        [HttpDelete("eliminar/{idExamen}")]
        public async Task<IActionResult> EliminarExamen(int idExamen)
        {
            var examen = await _context.Examenes.FindAsync(idExamen);
            if (examen == null)
            {
                return NotFound(new { message = "No se encontró el examen especificado." });
            }

            _context.Examenes.Remove(examen);
            await _context.SaveChangesAsync();

            return Ok(new { message = "El examen ha sido eliminado del calendario correctamente." });
        }
        [HttpGet("tipos-examen")]
        public async Task<IActionResult> GetTiposExamen()
        {
            var tipos = await _context.TipoExamen
                .Select(t => new
                {
                    idTipoExamen = t.IdTipoExamen,
                    nombre = t.TipoExamen // Propiedad mapeada en su modelo
                })
                .ToListAsync();

            return Ok(tipos);
        }

        [HttpGet("mis-materias-docente")]
        [Authorize(Roles = "Docente")]
        public async Task<IActionResult> ObtenerMisMateriasDocente()
        {
            // 1. Extraemos el ID del usuario desde el Token JWT
            string? usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(usuarioIdClaim, out int idUsuario))
            {
                return Unauthorized(new { message = "Token inválido." });
            }

            // 2. Verificamos que este usuario realmente sea un Docente
            var docente = await _context.Docentes.FirstOrDefaultAsync(d => d.IdUsuario == idUsuario);

            if (docente == null)
            {
                return NotFound(new { message = "El usuario no tiene un perfil de docente." });
            }

            // 3. Cruzamos la tabla intermedia con Materias y Comisiones
            var materiasAsignadas = await (
                from dm in _context.DocenteMateria
                join m in _context.Materias on dm.IdMateria equals m.IdMateria
                join c in _context.Comisions on dm.IdComision equals c.IdComision
                where dm.IdDocente == docente.IdDocente
                select new
                {
                    IdMateria = m.IdMateria,
                    NombreMateria = m.Nombre,
                    Carrera = m.Carrera,
                    IdComision = c.IdComision,
                    NombreComision = c.Comision1
                }
            ).Distinct().ToListAsync();

            return Ok(materiasAsignadas);
        }

    }
}

