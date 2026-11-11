namespace AutoGestionAPI.Models;

/// <summary>
/// Los tipos de examen del calendario (tabla tipo_examen). Se buscan por nombre:
/// el id puede variar de una base a otra.
/// </summary>
public static class TiposExamen
{
    public const string Parcial = "Parcial";
    public const string Recuperatorio = "Recuperatorio";
    public const string Final = "Final";

    /// <summary>
    /// Las mesas que registra Dirección/Secretaría. Son por materia (y por lo tanto
    /// por año), para todas las comisiones: se guardan sin comisión.
    /// </summary>
    public const string MesaDeExamen = "Mesa de Examen";

    /// <summary>Los que carga un docente desde el calendario.</summary>
    public static readonly string[] DelDocente = { Parcial, Recuperatorio, Final };

    public static readonly string[] Todos = { Parcial, Recuperatorio, Final, MesaDeExamen };

    /// <summary>Lo que se muestra en lugar de la comisión cuando el examen es de todas.</summary>
    public const string TodasLasComisiones = "Todas las comisiones";
}
