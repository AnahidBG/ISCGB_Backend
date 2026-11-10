namespace AutoGestionAPI.Seguridad
{
    /// <summary>
    /// Validaciones comunes para archivos subidos por los usuarios.
    /// No alcanza con mirar el ContentType o la extensión (los manda el cliente
    /// y se pueden falsificar): también se revisan los primeros bytes del archivo.
    /// </summary>
    public static class ValidadorArchivos
    {
        public const long TamanoMaximoBytes = 10 * 1024 * 1024; // 10 MB

        private static readonly Dictionary<string, byte[][]> FirmasPorExtension = new()
        {
            [".pdf"] = new[] { new byte[] { 0x25, 0x50, 0x44, 0x46 } },              // %PDF
            [".png"] = new[] { new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
            [".jpg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
            [".jpeg"] = new[] { new byte[] { 0xFF, 0xD8, 0xFF } },
        };

        public static readonly string[] SoloPdf = { ".pdf" };
        public static readonly string[] Imagenes = { ".jpg", ".jpeg", ".png" };
        public static readonly string[] PdfOImagen = { ".pdf", ".jpg", ".jpeg", ".png" };

        /// <summary>
        /// Devuelve null si el archivo es válido, o el mensaje de error si no lo es.
        /// </summary>
        public static string? Validar(IFormFile? archivo, string[] extensionesPermitidas, long tamanoMaximo = TamanoMaximoBytes)
        {
            if (archivo == null || archivo.Length == 0)
                return "No se adjuntó ningún archivo.";

            if (archivo.Length > tamanoMaximo)
                return $"El archivo supera el tamaño máximo de {tamanoMaximo / (1024 * 1024)} MB.";

            string extension = ObtenerExtension(archivo);
            if (!extensionesPermitidas.Contains(extension))
                return $"Formato no válido. Formatos permitidos: {string.Join(", ", extensionesPermitidas)}.";

            if (!CoincideFirma(archivo, extension))
                return "El contenido del archivo no corresponde con su formato.";

            return null;
        }

        /// <summary>Extensión en minúsculas, con el punto (por ejemplo ".pdf").</summary>
        public static string ObtenerExtension(IFormFile archivo)
        {
            return Path.GetExtension(archivo.FileName).ToLowerInvariant();
        }

        /// <summary>
        /// Deja solo letras y números, sin acentos, para armar nombres de archivo
        /// (evita que un texto con "/" o ".." arme otra ruta).
        /// </summary>
        public static string LimpiarParaNombreArchivo(string? texto)
        {
            if (string.IsNullOrWhiteSpace(texto)) return string.Empty;

            // Quita acentos (convierte Á a A, ñ a n, etc.)
            string descompuesto = texto.Normalize(System.Text.NormalizationForm.FormD);
            System.Text.StringBuilder sinAcentos = new System.Text.StringBuilder();
            foreach (char c in descompuesto)
            {
                if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    sinAcentos.Append(c);
            }

            return System.Text.RegularExpressions.Regex.Replace(
                sinAcentos.ToString().Normalize(System.Text.NormalizationForm.FormC),
                @"[^\p{L}\p{N}]", "");
        }

        private static bool CoincideFirma(IFormFile archivo, string extension)
        {
            if (!FirmasPorExtension.TryGetValue(extension, out byte[][] firmas))
                return false;

            int largoMaximo = firmas.Max(f => f.Length);
            byte[] cabecera = new byte[largoMaximo];

            using Stream stream = archivo.OpenReadStream();
            int leidos = stream.Read(cabecera, 0, largoMaximo);

            return firmas.Any(firma =>
                leidos >= firma.Length && cabecera.Take(firma.Length).SequenceEqual(firma));
        }
    }
}
