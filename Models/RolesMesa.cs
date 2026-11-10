namespace AutoGestionAPI.Models;

/// <summary>
/// Los tres lugares del tribunal de una mesa de examen. Cada mesa se registra
/// con un docente en cada lugar, y aparece en el calendario cuando los tres aceptan.
/// </summary>
public static class RolesMesa
{
    public const string Presidente = "Presidente";
    public const string Vocal1 = "Vocal 1";
    public const string Vocal2 = "Vocal 2";

    public static readonly string[] Todos = { Presidente, Vocal1, Vocal2 };

    /// <summary>Devuelve el rol con su escritura oficial, o null si no es válido.</summary>
    public static string? Normalizar(string? rol)
    {
        if (string.IsNullOrWhiteSpace(rol)) return null;
        string limpio = rol.Trim().Replace(" ", "").ToLowerInvariant();
        return limpio switch
        {
            "presidente" => Presidente,
            "vocal1" => Vocal1,
            "vocal2" => Vocal2,
            _ => null
        };
    }
}
