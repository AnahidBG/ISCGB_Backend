namespace AutoGestionAPI.DTOs
{
    // Usado por Dirección/Secretaría para cubrir un lugar del tribunal (por ejemplo,
    // después de que un docente rechazó la mesa)
    public class AsignarMesaDto
    {
        public int IdExamen { get; set; }
        public int IdDocente { get; set; }

        /// <summary>
        /// "Presidente", "Vocal 1" o "Vocal 2". Opcional si queda un solo lugar libre:
        /// en ese caso se cubre ese.
        /// </summary>
        public string? Rol { get; set; }
    }

    // Usado por el Docente para responder
    public class ResponderMesaDto
    {
        public int IdExamen { get; set; }
        public bool Acepta { get; set; } // true = Aceptada, false = Rechazada
    }
    public class OcultarAlertaDto
    {
        public int IdExamen { get; set; }
        public int IdDocente { get; set; }
    }
}