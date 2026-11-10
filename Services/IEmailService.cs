using System.Net;
using System.Net.Mail;

namespace AutoGestionAPI.Services
{
    // 1. LA INTERFAZ: Acá van solo las firmas (terminan con punto y coma)
    public interface IEmailService
    {
        Task EnviarLinkConfiguracionAsync(string emailDestino, string nombre, string urlConfiguracion);

        Task EnviarAvisoFaltantesAsync(string email, string nombre, List<string> documentosFaltantes);
    }

    // 2. LA CLASE: Acá va el código real de cada función (terminan con llaves { })
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _entorno;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, IHostEnvironment entorno, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _entorno = entorno;
            _logger = logger;
        }

        // Cuenta remitente. La contraseña NO va en el código ni en appsettings.json:
        // se carga con user-secrets (desarrollo) o con la variable de entorno
        // Email__Password (producción).
        private string Remitente => _configuration["Email:Usuario"]
            ?? throw new InvalidOperationException("Falta configurar 'Email:Usuario'. Ver README.");

        public async Task EnviarLinkConfiguracionAsync(string emailDestino, string nombre, string urlConfiguracion)
        {
            string nombreHtml = WebUtility.HtmlEncode(nombre);
            string urlHtml = WebUtility.HtmlEncode(urlConfiguracion);

            string cuerpo = $"<h3>Hola {nombreHtml}</h3><p>Hacé clic en el siguiente enlace para crear tu contraseña de acceso al sistema: <br><br> <a href='{urlHtml}'>Configurar mi contraseña</a></p>";

            await EnviarAsync(emailDestino, "Configura tu contraseña", cuerpo,
                resumenConsola: $"Link: {urlConfiguracion}");
        }

        public async Task EnviarAvisoFaltantesAsync(string email, string nombre, List<string> documentosFaltantes)
        {
            string nombreHtml = WebUtility.HtmlEncode(nombre);

            // Armamos la lista de viñetas HTML con los documentos
            string listaHtml = "";
            foreach (string doc in documentosFaltantes)
            {
                listaHtml += $"<li><strong>{WebUtility.HtmlEncode(doc)}</strong></li>";
            }

            string cuerpo = $@"
                    <div style='font-family: Arial, sans-serif; color: #333;'>
                        <h3>Hola {nombreHtml},</h3>
                        <p>Te contactamos desde la administración para recordarte que tenés documentación pendiente de entrega en tu legajo.</p>
                        <p>Actualmente adeudás los siguientes documentos:</p>
                        <ul>
                            {listaHtml}
                        </ul>
                        <p>Por favor, regularizá esta situación a la brevedad presentándolos en secretaría.</p>
                        <p>Saludos cordiales.</p>
                    </div>";

            await EnviarAsync(email, "Aviso de Documentación Faltante", cuerpo,
                resumenConsola: $"Documentos faltantes: {string.Join(", ", documentosFaltantes)}");
        }

        private async Task EnviarAsync(string destino, string asunto, string cuerpoHtml, string resumenConsola)
        {
            string? password = _configuration["Email:Password"];

            // En desarrollo, si no hay contraseña de Gmail cargada, el correo no se
            // manda: se muestra en la consola (así se pueden copiar los links).
            if (string.IsNullOrWhiteSpace(password))
            {
                if (!_entorno.IsDevelopment())
                    throw new InvalidOperationException("Falta configurar 'Email:Password'. Ver README.");

                _logger.LogWarning(
                    "[CORREO NO ENVIADO - sin Email:Password] Para: {Destino} | Asunto: {Asunto} | {Resumen}",
                    destino, asunto, resumenConsola);
                return;
            }

            using SmtpClient smtpClient = new SmtpClient(_configuration["Email:Host"] ?? "smtp.gmail.com")
            {
                Port = int.TryParse(_configuration["Email:Puerto"], out int puerto) ? puerto : 587,
                Credentials = new NetworkCredential(Remitente, password),
                EnableSsl = true,
            };

            using MailMessage mensaje = new MailMessage
            {
                From = new MailAddress(Remitente, "Sistema Académico"),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true,
            };

            mensaje.To.Add(destino);
            await smtpClient.SendMailAsync(mensaje);
        }
    }
}
