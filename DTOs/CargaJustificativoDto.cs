using Microsoft.AspNetCore.Http;
using System;

namespace AutoGestionAPI.DTOs
{
    public class CargarJustificativoDto
    {
        public int IdUsuario { get; set; }
        public string TipoInasistencia { get; set; } = null!;
        public string? NotaAdicional { get; set; }
        public DateTime? FechaInasistenciaInicio { get; set; }
        public DateTime? FechaInasistenciaFin { get; set; }
        
        // El archivo que viene desde Angular
        public IFormFile? DocumentoPdf { get; set; } 
    }
}