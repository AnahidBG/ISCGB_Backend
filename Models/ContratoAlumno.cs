using System;

namespace AutoGestionAPI.Models;

/// <summary>
/// Contrato firmado de un alumno (por ejemplo, el convenio de la beca) que carga
/// Dirección o Secretaría para que el alumno lo vea desde su panel.
/// </summary>
public partial class ContratoAlumno
{
    public int IdContrato { get; set; }

    /// <summary>El alumno al que pertenece el contrato.</summary>
    public int IdUsuario { get; set; }

    /// <summary>Nombre visible, por ejemplo "Convenio Beca Fundación Encode 2026".</summary>
    public string Nombre { get; set; } = null!;

    /// <summary>Nombre del archivo dentro de la carpeta privada de contratos.</summary>
    public string RutaArchivo { get; set; } = null!;

    public DateTime FechaCarga { get; set; }

    /// <summary>Quién lo cargó (Director o Secretario).</summary>
    public int IdUsuarioCarga { get; set; }

    public virtual Usuario IdUsuarioNavigation { get; set; } = null!;

    public virtual Usuario IdUsuarioCargaNavigation { get; set; } = null!;
}
