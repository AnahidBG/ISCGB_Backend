namespace AutoGestionAPI.DTOs
{
    // Usado por el Director para asignar la mesa
    public class AsignarMesaDto
    {
        public int IdExamen { get; set; }
        public int IdDocente { get; set; }
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