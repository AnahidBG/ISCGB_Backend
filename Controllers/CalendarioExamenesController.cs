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
        [Authorize(Roles = "Director, Secretario")]
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
        [Authorize(Roles = "Docente")] // Aseguramos que solo entre un docente
        public async Task<IActionResult> RegistrarExamen([FromBody] CargaExamenDto dto)
        {
            // 1. Identificamos al docente logueado
            string? usuarioIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(usuarioIdClaim, out int idUsuario)) return Unauthorized();

            var docente = await _context.Docentes.FirstOrDefaultAsync(d => d.IdUsuario == idUsuario);
            if (docente == null) return NotFound(new { message = "Docente no encontrado." });

            // 2. MAGIA DE SEGURIDAD: Verificamos en la tabla docente_materia si realmente da esa clase
            // (Ajuste "DocenteMaterias" al nombre exacto que le haya puesto Entity Framework a su tabla intermedia)
            bool dictaLaMateria = await _context.DocenteMateria
                .AnyAsync(dm => dm.IdDocente == docente.IdDocente && dm.IdMateria == dto.IdMateria);

            if (!dictaLaMateria)
            {
                return BadRequest(new { message = "No tiene autorización para registrar exámenes de una materia que no dicta." });
            }

            // 3. Verificamos el límite diario que ya teníamos programado
            bool superaLimite = await SuperaLimiteDiarioAsync(dto.Fecha);
            if (superaLimite)
            {
                return BadRequest(new { message = "No se puede registrar. Se ha superado el límite de parciales para ese día." });
            }

            // 4. Guardamos el examen (tal como lo tenía)
            var nuevoExamen = new Examene
            {
                Fecha = dto.Fecha,
                IdComision = dto.IdComision,
                IdMateria = dto.IdMateria,
                IdTipoExamen = dto.IdTipoExamen
            };

            _context.Examenes.Add(nuevoExamen);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Examen registrado correctamente.",
                idExamen = nuevoExamen.IdExamen
            });
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
                    nombre = t.TipoExamen
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

        [HttpGet("del-mes")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ExamenMesDto>>> ObtenerExamenesDelMes()
        {
            int mesActual = DateTime.Now.Month;
            int anioActual = DateTime.Now.Year;

            var examenesMes = await _context.Examenes
                .Include(e => e.IdMateriaNavigation)
                .Include(e => e.IdComisionNavigation)    // NUEVO: Traemos los datos de la Comisión
                .Include(e => e.IdTipoExamenNavigation)  // NUEVO: Traemos los datos del Tipo de Examen
                .Where(e => e.Fecha.Month == mesActual && e.Fecha.Year == anioActual)
                .Select(e => new ExamenMesDto
                {
                    IdExamen = e.IdExamen,
                    Materia = e.IdMateriaNavigation.Nombre,
                    Fecha = e.Fecha,

                    // Ajuste ".Nombre" por el campo exacto que tenga en sus clases (ej. .Descripcion, .NumeroComision, etc.)
                    Comision = e.IdComisionNavigation.Comision1,
                    TipoExamen = e.IdTipoExamenNavigation.TipoExamen
                })
                .OrderBy(e => e.Fecha)
                .ToListAsync();

            if (!examenesMes.Any())
            {
                return Ok(new List<ExamenMesDto>());
            }

            return Ok(examenesMes);
        }

        // --- 3. ENDPOINT PARA ASIGNAR DOCENTE A MESA DE EXAMEN YA CREADA ---
        [HttpPost("asignar-docente")]
        [Authorize(Roles = "Director,Administrador")]
        public async Task<IActionResult> AsignarDocenteAMesa([FromBody] AsignarMesaDto dto)
        {
            // 1. Verificamos que no exista ya una asignación para este examen y este docente
            bool asignacionExiste = await _context.MesasExamenes
                .AnyAsync(m => m.IdExamen == dto.IdExamen && m.IdDocente == dto.IdDocente);

            if (asignacionExiste)
            {
                return BadRequest(new { message = "El docente ya se encuentra asignado a esta mesa de examen." });
            }

            // 2. Creamos la asignación
            MesaExamen nuevaAsignacion = new MesaExamen // Ajuste el nombre exacto de su clase si es singular o plural
            {
                IdExamen = dto.IdExamen,
                IdDocente = dto.IdDocente,
                EstadoConfirmacion = "Pendiente" // Estado inicial automático
            };

            _context.MesasExamenes.Add(nuevaAsignacion);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Docente asignado correctamente. Se ha notificado al profesor." });
        }

        //Endpoint para crear mesa de examen y asignar al presidente de mesa.

        [HttpPost("registrar-mesa")]
        [Authorize(Roles = "Director,Secretario")]
        public async Task<IActionResult> RegistrarMesa([FromBody] RegistrarMesaDto dto)
        {

            bool docenteExiste = await _context.Docentes.AnyAsync(d => d.IdDocente == dto.IdDocente);
            if (!docenteExiste)
            {
                return BadRequest(new { message = "El docente seleccionado no existe." });
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Paso A: Insertamos los datos de la fecha y materia en Examenes
                var nuevaMesa = new Examene
                {
                    Fecha = dto.Fecha,
                    IdMateria = dto.IdMateria,
                    IdComision = dto.IdComision,
                    IdTipoExamen = dto.IdTipoExamen
                };

                _context.Examenes.Add(nuevaMesa);
                await _context.SaveChangesAsync();


                var asignacionDocente = new MesaExamen
                {
                    IdExamen = nuevaMesa.IdExamen,
                    IdDocente = dto.IdDocente,
                    EstadoConfirmacion = "Pendiente"
                };

                _context.MesasExamenes.Add(asignacionDocente);
                await _context.SaveChangesAsync();

                // Confirmamos definitivamente ambos cambios en SQL Server
                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Mesa de examen registrada exitosamente. Se ha enviado la notificación pendiente al docente.",
                    idExamen = nuevaMesa.IdExamen
                });
            }
            catch (Exception)
            {

                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Ocurrió un error al registrar la mesa de examen." });
            }
        }

    }
}

