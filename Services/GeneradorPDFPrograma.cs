using AutoGestionAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public static class GeneradorPdfPrograma
{
    public static IDocument CrearDocumento(ProgramasMaterium programa)
    {
        // QuestPDF requiere que indiques la licencia comunitaria gratis
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                // ENCABEZADO (Logos, Títulos, Datos del docente)
                page.Header().Column(col => 
                {
                    col.Item().Text("INSTITUTO SUPERIOR Cura Gabriel Brochero").Bold().FontSize(14);
                    col.Item().Text($"Carrera: Tecnicatura Superior en Desarrollo de Software");
                    col.Item().Text($"Unidad curricular: {programa.IdMateriaNavigation?.Nombre}");
                    col.Item().Text($"Curso: {programa.IdMateriaNavigation?.Curso}");
                    col.Item().Text($"Docente: {programa.IdDocenteNavigation?.IdUsuarioNavigation?.Apellido}, {programa.IdDocenteNavigation?.IdUsuarioNavigation?.Nombre}");
                    col.Item().Text($"Condición: {programa.Condicion}");
                    col.Item().Text($"Ciclo lectivo: {programa.CicloLectivo}");
                    col.Item().PaddingBottom(10);
                });

                // CONTENIDO PRINCIPAL
                page.Content().Column(col =>
                {
                    col.Item().Text("1. Fundamentación").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.Fundamentacion);
                    // Sección 2.1 Objetivos Generales
                    col.Item().Text("2.1. Objetivos generales").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.ObjetivosGenerales);

                    // Sección 3 Contenidos (Iteramos sobre la lista de la base de datos)
                    col.Item().Text("3. Contenidos").Bold().FontSize(12);
                    foreach (var unidad in programa.Contenidos)
                        {
                            col.Item().Text($"Unidad {unidad.Unidad}. {unidad.TituloUnidad}").SemiBold();
                            col.Item().Text($"Contenido: {unidad.Contenido1}");
                            col.Item().PaddingBottom(5).Text($"Bibliografía obligatoria: {unidad.BibliografiaObligatoria}");
                        }

                    // Sección 4 Estrategias
                    col.Item().Text("4. Estrategias metodológicas").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.EstrategiasMetodologicas);
                });

                // PIE DE PÁGINA
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        });
    }
}