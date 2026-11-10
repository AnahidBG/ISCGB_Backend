using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace AutoGestionAPI.Services
{
    /// <summary>
    /// Años (cursos) válidos de una materia. El año se usa para el límite diario de
    /// exámenes por año + comisión, así que tiene que estar escrito siempre igual:
    /// "1° año", "1er año" o "primer año" se guardan todos como "Primer Año".
    /// </summary>
    public static class CursosMateria
    {
        public static readonly string[] Validos = { "Primer Año", "Segundo Año", "Tercer Año" };

        /// <summary>
        /// Devuelve el año en su forma oficial, o null si el texto no corresponde a
        /// ningún año válido.
        /// </summary>
        public static string? Normalizar(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return null;

            // Minúsculas, sin acentos ni símbolos: "1° Año" → "1 ano", "Primer Año" → "primer ano"
            string limpio = SinAcentos(texto.ToLowerInvariant());
            limpio = Regex.Replace(limpio, @"[^a-z0-9]+", " ").Trim();
            limpio = Regex.Replace(limpio, @"\s*\bano\b$", "").Trim();   // la palabra "año" es opcional

            return limpio switch
            {
                "1" or "1 er" or "1er" or "1ro" or "1 ro" or "primer" or "primero" => Validos[0],
                "2" or "2 do" or "2do" or "segundo" => Validos[1],
                "3" or "3 er" or "3er" or "3ro" or "3 ro" or "tercer" or "tercero" => Validos[2],
                _ => null
            };
        }

        private static string SinAcentos(string texto)
        {
            StringBuilder resultado = new StringBuilder();
            foreach (char c in texto.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    resultado.Append(c);
            }
            return resultado.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}
