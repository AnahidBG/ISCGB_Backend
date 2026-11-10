namespace AutoGestionAPI.DTOs
{
    public class AuditarJustificativoDto
    {
        public int IdUsuarioAuditor { get; set; }
        public string Estado { get; set; } = null!;

        /// <summary>
        /// Opcional: el motivo del rechazo. No se guarda en la base; se incluye en el
        /// mail que recibe la persona.
        /// </summary>
        public string? Comentario { get; set; }
    }
}