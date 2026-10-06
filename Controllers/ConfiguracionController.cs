using Microsoft.AspNetCore.Mvc;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutoGestionAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionController : ControllerBase
    {
        private readonly TuDbContext _context;

        public ConfiguracionController(TuDbContext context)
        {
            _context = context;
        }

        [HttpGet("frecuencia-notificaciones")]
        public async Task<IActionResult> ObtenerFrecuencia()
        {
            var config = await _context.ConfiguracionesSistema.FirstOrDefaultAsync();

            int dias = config?.FrecuenciaNotificacionDias ?? 7;

            return Ok(new { diasFrecuencia = dias });
        }

        [HttpPut("frecuencia-notificaciones")]
        public async Task<IActionResult> ActualizarFrecuencia([FromBody] ConfiguracionNotificacionDto dto)
        {
            if (dto.DiasFrecuencia <= 0)
                return BadRequest(new { message = "La frecuencia debe ser mayor a 0 días." });

            var config = await _context.ConfiguracionesSistema.FirstOrDefaultAsync();

            if (config == null)
            {
                config = new ConfiguracionSistema { FrecuenciaNotificacionDias = dto.DiasFrecuencia };
                _context.ConfiguracionesSistema.Add(config);
            }
            else
            {
                config.FrecuenciaNotificacionDias = dto.DiasFrecuencia;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = $"Frecuencia actualizada a {dto.DiasFrecuencia} días exitosamente." });
        }
    }
}