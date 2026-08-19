using System;
using System.Collections.Generic;

namespace AutoGestionAPI.Models;

public partial class Legajo
{
    public int IdLegajo { get; set; }

    public int IdUsuario { get; set; }

    public int IdTipoDoc { get; set; }

    public int? IdUsuarioAuditor { get; set; }

    public string? RutaArchivo { get; set; }

    public DateTime FechaCarga { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public string? Estado { get; set; }

    public bool? PresentadoFisico { get; set; }

    public string? Comentario { get; set; }

    public virtual TiposDocumento IdTipoDocNavigation { get; set; } = null!;

    public virtual Usuario? IdUsuarioAuditorNavigation { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;
}
