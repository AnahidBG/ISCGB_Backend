using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace AutoGestionAPI.DTOs
{
    public class SolicitudReconocimientoDto
    {
        [Required]
        public int IdMateria { get; set; }

        public string? Comentario { get; set; }

        [Required]
        public IFormFile ProgramaPdf { get; set; } = null!;

        [Required]
        public IFormFile AnaliticoPdf { get; set; } = null!;
    }
}