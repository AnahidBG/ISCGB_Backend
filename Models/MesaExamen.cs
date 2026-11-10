using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class MesaExamen
{
    public int IdExamen { get; set; }
    public int IdDocente { get; set; }

    // Aquí guardaremos: "Pendiente", "Aceptada", "Rechazada" o "Rechazada_Vista"
    public string? EstadoConfirmacion { get; set; } = "Pendiente";

    /// <summary>
    /// Lugar del docente en el tribunal: "Presidente", "Vocal 1" o "Vocal 2"
    /// (ver <see cref="RolesMesa"/>).
    /// </summary>
    public string Rol { get; set; } = RolesMesa.Presidente;

    public virtual Docente IdDocenteNavigation { get; set; } = null!;
    public virtual Examene IdExamenNavigation { get; set; } = null!;
}