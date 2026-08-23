using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoGestionAPI.Models;
using AutoGestionAPI.DTOs;
using QuestPDF.Fluent;

namespace AutoGestionAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProgramasMateriaController : ControllerBase
    {
        private readonly TuDbContext _context; 

        public ProgramasMateriaController(TuDbContext context)
        {
            _context = context;
        }

        // 1. POST: Crear un nuevo programa de materia con sus contenidos
        [HttpPost]
        public async Task<IActionResult> CrearPrograma([FromBody] CrearProgramaDto dto)
        {
            var docenteExiste = await _context.Docentes.AnyAsync(d => d.IdDocente == dto.IdDocente);
            if (!docenteExiste)
                return NotFound(new { message = "El docente especificado no existe." });

            var materiaExiste = await _context.Materias.AnyAsync(m => m.IdMateria == dto.IdMateria);
            if (!materiaExiste)
                return NotFound(new { message = "La materia especificada no existe." });

            var nuevoPrograma = new ProgramasMaterium
            {
                IdDocente = dto.IdDocente,
                IdMateria = dto.IdMateria,
                Condicion = dto.Condicion,
                ObjetivosEspecificos = dto.ObjetivosEspecificos,
                ObjetivosGenerales = dto.ObjetivosGenerales,
                HorasSemanales = dto.HorasSemanales,
                HorasCuatrimestrales = dto.HorasCuatrimestrales,
                Evaluacion = dto.Evaluacion,
                CriteriosEvaluacion = dto.CriteriosEvaluacion,
                EstrategiasMetodologicas = dto.EstrategiasMetodologicas,
                EstrategiasAcompanamientoVirtualRemoto = dto.EstrategiasAcompanamientoVirtualRemoto,
                CondicionRegular = dto.CondicionRegular,
                CondicionPromocional = dto.CondicionPromocional,
                CondicionLibre = dto.CondicionLibre,
                ExamenesVirtuales = dto.ExamenesVirtuales,
                FormatoCurricular = dto.FormatoCurricular,
                CicloLectivo = dto.CicloLectivo,
                Fundamentacion = dto.Fundamentacion,
                

                Contenidos = dto.Contenidos.Select(c => new Contenido 
                {
                    Unidad = c.Unidad,
                    TituloUnidad = c.TituloUnidad,
                    Contenido1 = c.Contenido, 
                    BibliografiaObligatoria = c.BibliografiaObligatoria,
                    BibliografiaComplementaria = c.BibliografiaComplementaria
                }).ToList()
            };


            _context.ProgramasMateria.Add(nuevoPrograma);
            await _context.SaveChangesAsync();

            return Ok(new { 
                message = "Programa y contenidos guardados con éxito.", 
                idPrograma = nuevoPrograma.IdPrograma 
            });
        }

        [HttpGet("{idPrograma}/pdf")]
public async Task<IActionResult> DescargarPdf(int idPrograma)
{

var programa = await _context.ProgramasMateria
    .Include(p => p.Contenidos)
    // Buscamos la Materia
    .Include(p => p.IdMateriaNavigation) 

    .Include(p => p.IdDocenteNavigation)
        .ThenInclude(d => d.IdUsuarioNavigation)
    .FirstOrDefaultAsync(p => p.IdPrograma == idPrograma);

    if (programa == null) return NotFound();


    var documentoPdf = GeneradorPdfPrograma.CrearDocumento(programa);
    

    var pdfBytes = documentoPdf.GeneratePdf();


    return File(pdfBytes, "application/pdf", $"Programa_Materia_{programa.IdMateria}.pdf");
}
    }
}