using Microsoft.AspNetCore.Http;

namespace AutoGestionAPI.DTOs
{
    public class SubirFotoDto
    {
        // Esta propiedad recibirá el archivo físico que envíe Angular
        public IFormFile Foto { get; set; }
    }
}