using Microsoft.AspNetCore.Http;
using System;

namespace AutoGestionAPI.DTOs
{
    public class SubirLegajoDto
    {
        public int IdUsuario { get; set; }
        public int IdTipoDoc { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public bool PresentadoFisico { get; set; }
        
        public IFormFile Archivo { get; set; } = null!;
    }
}