using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class Justificativo
{
    public int IdJustificativo { get; set; }

    public int IdUsuario { get; set; }

    public int? IdUsuarioAuditor { get; set; }

    public string? TipoInasistencia { get; set; }

    public string? RutaArchivo { get; set; }

    public string? NotaAdicional { get; set; }

    public DateTime FechaCarga { get; set; }

    public string? Estado { get; set; }

    public DateTime? FechaInasistenciaInicio { get; set; }

    public DateTime? FechaInasistenciaFin { get; set; }

    public virtual Usuario? IdUsuarioAuditorNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
