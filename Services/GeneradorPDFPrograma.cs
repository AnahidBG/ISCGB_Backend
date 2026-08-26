using AutoGestionAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public static class GeneradorPdfPrograma
{
    public static IDocument CrearDocumento(ProgramasMaterium programa)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                // Encabezado
                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("Ministerio de Educación.").FontSize(10);
                    col.Item().AlignCenter().Text("Dirección General de Institutos Privados de Enseñanza").FontSize(10);
                    col.Item().AlignCenter().Text("Instituto Superior Cura Gabriel Brochero").Bold().FontSize(12);
                    col.Item().PaddingTop(15);

                    col.Item().Text(text =>
                    {
                        text.Span("Carrera: ").SemiBold();
                        text.Span("Tecnicatura Superior en Desarrollo de Software");
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Unidad curricular: ").SemiBold();
                        text.Span($"{programa.IdMateriaNavigation?.Nombre}");
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Formato Curricular: ").SemiBold();
                        text.Span($"{programa.FormatoCurricular}");
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Curso: ").SemiBold();
                        text.Span($"{programa.IdMateriaNavigation?.Curso} ");
                        text.Span("Horas cátedra: ").SemiBold();
                        text.Span($"{programa.HorasSemanales} semanales, {programa.HorasCuatrimestrales} cuatrimestrales");
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Condición: ").SemiBold();
                        text.Span($"{programa.Condicion}");
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Ciclo lectivo: ").SemiBold();
                        text.Span($"{programa.CicloLectivo}");
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Docente: ").SemiBold();
                        text.Span($"{programa.IdDocenteNavigation?.IdUsuarioNavigation?.Apellido}, {programa.IdDocenteNavigation?.IdUsuarioNavigation?.Nombre}");
                    });
                    col.Item().PaddingBottom(15);
                });

                // CONTENIDO DEL PROGRAMA
                page.Content().Column(col =>
                {
                    // 1. Fundamentación
                    col.Item().Text("1. Fundamentación.").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.Fundamentacion);

                    // 2. Objetivos
                    col.Item().Text("2.1. Objetivos generales").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.ObjetivosGenerales);
                    col.Item().Text("2.2. Objetivos específicos").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.ObjetivosEspecificos);

                    // 3. Contenidos
                    col.Item().Text("3. Contenidos").Bold().FontSize(12);
                    foreach (var unidad in programa.Contenidos)
                    {
                        col.Item().PaddingTop(5).Text($"Unidad {unidad.Unidad}. {unidad.TituloUnidad}").SemiBold();
                        col.Item().Text("Contenido:").SemiBold();
                        col.Item().Text($"{unidad.Contenido1}");
                        col.Item().Text("Bibliografia obligatoria:").SemiBold();
                        col.Item().Text($"{unidad.BibliografiaObligatoria}");
                        col.Item().Text("Bibliografia complementaria:").SemiBold();
                        col.Item().PaddingBottom(10).Text($"{unidad.BibliografiaComplementaria}");
                    }

                    // Evaluación
                    col.Item().Text("Evaluación:").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.Evaluacion);
                    col.Item().Text("Criterios de evaluación:").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.CriteriosEvaluacion);

                    // 4. Estrategias metodológicas
                    col.Item().Text("4. Estrategias metodológicas:").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.EstrategiasMetodologicas);

                    // 5. Estrategias de acompañamiento virtual o remoto
                    col.Item().Text("5. Estrategias de acompañamiento virtual o remoto").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.EstrategiasAcompanamientoVirtualRemoto);

                    // 6. Condiciones de cursado y acreditación
                    col.Item().Text("6. Condiciones de cursado y acreditación del taller").Bold().FontSize(12);
                    col.Item().Text("Para alumnos/as regulares:").SemiBold();
                    col.Item().PaddingBottom(5).Text(programa.CondicionRegular);
                    col.Item().Text("Para alumnos/as promocionales:").SemiBold();
                    col.Item().PaddingBottom(5).Text(programa.CondicionPromocional);
                    col.Item().Text("Alumnos en Condición Libre:").SemiBold();
                    col.Item().PaddingBottom(10).Text(programa.CondicionLibre);

                    // 7. Exámenes virtuales
                    col.Item().Text("7. Exámenes virtuales:").Bold().FontSize(12);
                    col.Item().PaddingBottom(30).Text(programa.ExamenesVirtuales);

                    // Seccion de firmas
                    col.Item().Table(tabla =>
                    {
                        tabla.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Columna Izquierda (Sec. Académico)
                        tabla.Cell().AlignCenter().Column(c =>
                        {
                            c.Item().PaddingTop(40).LineHorizontal(1).LineColor(Colors.Black);
                            c.Item().Text("Firma y sello Sec. Académico").FontSize(10);
                            c.Item().Text("(sello institucional)").FontSize(10);
                        });

                        // Columna Derecha (Profesor)
                        tabla.Cell().PaddingLeft(50).AlignCenter().Column(c =>
                        {
                            c.Item().PaddingTop(40).LineHorizontal(1).LineColor(Colors.Black);
                            c.Item().Text("Firma del Profesor/a").FontSize(10);
                            c.Item().Text("Aclaración").FontSize(10);
                            c.Item().PaddingTop(10).Text(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(10);
                        });
                    });
                });

                // Pie de pagina
                page.Footer().Column(col =>
                {
                    col.Item().AlignCenter().Text("direccion@icgb.com.ar").FontSize(9).FontColor(Colors.Blue.Darken2);
                    col.Item().AlignCenter().Text("secretaria.academica@icgb.com.ar").FontSize(9).FontColor(Colors.Blue.Darken2);
                    col.Item().AlignCenter().Text("preceptoria@icgb.com.ar").FontSize(9).FontColor(Colors.Blue.Darken2);
                });
            });
        });
    }
}
