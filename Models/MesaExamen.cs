using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class MesaExamen
{
    public int IdExamen { get; set; }
    public int IdDocente { get; set; }

    // Aquí guardaremos: "Pendiente", "Aceptado", o "Rechazado"
    public string? EstadoConfirmacion { get; set; } = "Pendiente";

    public virtual Docente IdDocenteNavigation { get; set; } = null!;
    public virtual Examene IdExamenNavigation { get; set; } = null!;
}