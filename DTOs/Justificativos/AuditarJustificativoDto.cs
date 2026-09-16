namespace AutoGestionAPI.DTOs
{
    public class AuditarJustificativoDto
    {
        public int IdUsuarioAuditor { get; set; }
        public string Estado { get; set; } = null!; 
    }
}