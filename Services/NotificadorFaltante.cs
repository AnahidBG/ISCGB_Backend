using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.Services;

namespace AutoGestionAPI.Workers
{
    public class NotificadorFaltantesWorker : BackgroundService
    {
        // CAMBIO CLAVE ARCHITECTÓNICO: Usamos IServiceScopeFactory en lugar de IServiceProvider
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<NotificadorFaltantesWorker> _logger;

        public NotificadorFaltantesWorker(IServiceScopeFactory scopeFactory, ILogger<NotificadorFaltantesWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Motor de notificaciones iniciado (Versión Inteligente con Roles y Obligatoriedades).");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. CREAMOS UN ÁMBITO (SCOPE) NUEVO EN CADA ITERACIÓN
                    using (IServiceScope scope = _scopeFactory.CreateScope())
                    {
                        // 2. EXTRAEMOS LOS SERVICIOS FRESCOS Y LISTOS PARA USAR
                        TuDbContext context = scope.ServiceProvider.GetRequiredService<TuDbContext>();
                        IEmailService emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                        ConfiguracionSistema? config = await context.ConfiguracionesSistema.OrderBy(c => c.IdConfiguracion).FirstOrDefaultAsync(stoppingToken);
                        int diasFrecuencia = config?.FrecuenciaNotificacionDias ?? 7;
                        DateTime fechaLimite = DateTime.UtcNow.AddDays(-diasFrecuencia);

                        // Traemos los catálogos para no ir a la base de datos a cada rato
                        List<TiposDocumento> todosLosTiposDocumentos = await context.TiposDocumentos.ToListAsync(stoppingToken);

                        // Traemos SOLAMENTE las reglas donde Obligatorio es verdadero
                        List<RolesTiposDocumento> reglasDocumentos = await context.RolesTiposDocumentos
                            .Where(r => r.Obligatorio == true)
                            .ToListAsync(stoppingToken);

                        // Buscamos a los usuarios que necesitan revisión.
                        // Los dados de baja (EstadoUsuario = false) no reciben avisos.
                        List<Usuario> usuarios = await context.Usuarios
                            .Include(u => u.LegajoIdUsuarioNavigations)
                            .Include(u => u.UsuariosRoles)
                            .Where(u => u.EstadoUsuario)
                            .Where(u => u.FechaUltimaNotificacion == null || u.FechaUltimaNotificacion < fechaLimite)
                            .AsSplitQuery()
                            .ToListAsync(stoppingToken);

                        int usuariosNotificados = 0;

                        foreach (Usuario? usuario in usuarios)
                        {
                            List<int> idsRolesUsuario = usuario.UsuariosRoles.Select(ur => ur.IdRol).ToList();

                            List<int> idsDocumentosExigidos = reglasDocumentos
                                .Where(r => idsRolesUsuario.Contains(r.IdRol))
                                .Select(r => r.IdTipoDoc)
                                .Distinct()
                                .ToList();

                            List<int> idsSubidos = usuario.LegajoIdUsuarioNavigations
                                .Select(l => l.IdTipoDoc)
                                .ToList();

                            List<int> idsFaltantes = idsDocumentosExigidos.Except(idsSubidos).ToList();

                            if (idsFaltantes.Any())
                            {
                                List<string> nombresDocumentosFaltantes = todosLosTiposDocumentos
                                    .Where(td => idsFaltantes.Contains(td.IdTipoDoc))
                                    .Select(td => td.NombreDocumento)
                                    .ToList();

                                try
                                {
                                    await emailService.EnviarAvisoFaltantesAsync(usuario.Email, usuario.Nombre ?? "Usuario", nombresDocumentosFaltantes);
                                    usuario.FechaUltimaNotificacion = DateTime.UtcNow;
                                    usuariosNotificados++;
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogError(ex, $"No se pudo enviar el correo a {usuario.Email}");
                                }
                            }
                        }

                        if (usuariosNotificados > 0)
                        {
                            await context.SaveChangesAsync(stoppingToken);
                            _logger.LogInformation($"Se enviaron {usuariosNotificados} avisos de documentación faltante.");
                        }
                    } // <- Aquí el 'using' cierra y destruye el contexto limpiamente en cada ciclo
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ocurrió un error al ejecutar el motor de notificaciones.");
                }

                // Dejado en 1 minuto para sus pruebas
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}