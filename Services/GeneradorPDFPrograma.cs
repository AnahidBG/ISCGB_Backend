using AutoGestionAPI.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public static class GeneradorPdfPrograma
{
    public static IDocument CrearDocumento(ProgramasMaterium programa)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        byte[] encabezado = ObtenerEncabezado();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                // Encabezado
                page.Header().Column(col =>
                {
                    col.Item().Image(encabezado);

                });

                // CONTENIDO DEL PROGRAMA
                page.Content().PaddingHorizontal(2, Unit.Centimetre).PaddingBottom(1, Unit.Centimetre).Column(col =>
                {
                    col.Item().PaddingTop(10).AlignCenter().Text("Ministerio de Educación.").FontSize(10);
                    col.Item().AlignCenter().Text("Dirección General de Institutos Privados de Enseñanza").FontSize(10);
                    col.Item().AlignCenter().Text("Instituto Superior Cura Gabriel Brochero").Bold().FontSize(12).Italic();
                    col.Item().PaddingTop(15);

                    col.Item().Text(text =>
                    {
                        text.Span("Carrera: ");
                        text.Span("Tecnicatura Superior en Desarrollo de Software").SemiBold();
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Unidad curricular: ");
                        text.Span($"{programa.IdMateriaNavigation?.Nombre}").SemiBold();
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Formato Curricular: ");
                        text.Span($"{programa.FormatoCurricular}").SemiBold();
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Curso: ");
                        text.Span($"{programa.IdMateriaNavigation?.Curso} ").SemiBold();
                        text.Span("Horas cátedra: ");
                        text.Span($"{programa.HorasSemanales} semanales, {programa.HorasCuatrimestrales} cuatrimestrales").SemiBold();
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Condición: ");
                        text.Span($"{programa.Condicion}").SemiBold();
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Ciclo lectivo: ");
                        text.Span($"{programa.CicloLectivo}").SemiBold();
                    });
                    col.Item().Text(text =>
                    {
                        text.Span("Docente: ");
                        text.Span($"{programa.IdDocenteNavigation?.IdUsuarioNavigation?.Apellido}, {programa.IdDocenteNavigation?.IdUsuarioNavigation?.Nombre}").SemiBold();
                    });
                    col.Item().PaddingBottom(15);

                    // 1. Fundamentación
                    col.Item().PaddingBottom(5).Text("1. Fundamentación.").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.Fundamentacion);

                    // 2. Objetivos
                    col.Item().PaddingBottom(5).Text("2.1. Objetivos generales").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.ObjetivosGenerales);
                    col.Item().PaddingBottom(5).Text("2.2. Objetivos específicos").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.ObjetivosEspecificos);

                    // 3. Contenidos
                    col.Item().Text("3. Contenidos").Bold().FontSize(12);
                    foreach (var unidad in programa.Contenidos)
                    {
                        col.Item().PaddingTop(5).PaddingBottom(5).Text($"Unidad {unidad.Unidad}. {unidad.TituloUnidad}").SemiBold();
                        col.Item().PaddingBottom(5).Text("Contenido:").Underline();
                        col.Item().PaddingBottom(10).Text($"{unidad.Contenido1}");
                        col.Item().PaddingBottom(5).Text("Bibliografia obligatoria:").Underline();
                        col.Item().PaddingBottom(10).Text($"{unidad.BibliografiaObligatoria}");
                        col.Item().PaddingBottom(5).Text("Bibliografia complementaria:").Underline();
                        col.Item().PaddingBottom(10).Text($"{unidad.BibliografiaComplementaria}");
                    }

                    // Evaluación
                    col.Item().PaddingBottom(5).Text("Evaluación:").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.Evaluacion);
                    col.Item().PaddingBottom(5).Text("Criterios de evaluación:").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.CriteriosEvaluacion);

                    // 4. Estrategias metodológicas
                    col.Item().PaddingBottom(5).Text("4. Estrategias metodológicas:").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.EstrategiasMetodologicas);

                    // 5. Estrategias de acompañamiento virtual o remoto
                    col.Item().PaddingBottom(5).Text("5. Estrategias de acompañamiento virtual o remoto").Bold().FontSize(12);
                    col.Item().PaddingBottom(10).Text(programa.EstrategiasAcompanamientoVirtualRemoto);

                    // 6. Condiciones de cursado y acreditación
                    col.Item().PaddingBottom(10).Text("6. Condiciones de cursado y acreditación del taller").Bold().FontSize(12);
                    col.Item().PaddingBottom(5).Text("Para alumnos/as regulares:").SemiBold();
                    col.Item().PaddingBottom(10).Text(programa.CondicionRegular);
                    col.Item().PaddingBottom(5).Text("Para alumnos/as promocionales:").SemiBold();
                    col.Item().PaddingBottom(10).Text(programa.CondicionPromocional);
                    col.Item().Text("Alumnos en Condición Libre:").SemiBold();
                    col.Item().PaddingBottom(10).Text(programa.CondicionLibre);

                    // 7. Exámenes virtuales
                    col.Item().PaddingBottom(5).Text("7. Exámenes virtuales:").Bold().FontSize(12);
                    col.Item().PaddingBottom(30).Text(programa.ExamenesVirtuales);

                    // Seccion de firmas
                    col.Item().PaddingTop(50).Table(tabla =>
                    {
                        tabla.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        // Columna Izquierda (Sec. Académico)
                        tabla.Cell().PaddingRight(40).Column(c =>
                        {
                            c.Item().LineHorizontal(1).LineColor(Colors.Black);
                            c.Item().AlignCenter().Text("Firma y sello Sec. Académico").FontSize(10);

                            // El salto de línea hacia el sello
                            c.Item().PaddingTop(55).AlignCenter().Text("(sello institucional)").FontSize(10);
                        });

                        // Columna Derecha (Profesor)
                        tabla.Cell().PaddingLeft(40).Column(c =>
                        {
                            // Firma superior
                            c.Item().LineHorizontal(1).LineColor(Colors.Black);
                            c.Item().AlignCenter().Text("Firma del Profesor/a").FontSize(10);

                            // Segunda línea para la aclaración
                            c.Item().PaddingTop(40).LineHorizontal(1).LineColor(Colors.Black);
                            c.Item().AlignCenter().Text("Aclaración").FontSize(10);

                            // Fecha
                            c.Item().PaddingTop(5).AlignCenter().Text(DateTime.Now.ToString("dd/MM/yyyy")).FontSize(10);
                        });
                    });
                });

                // Pie de pagina
                page.Footer().Background("#4f6959").PaddingVertical(8).PaddingHorizontal(2, Unit.Centimetre).Row(row =>
                {
                    string colorTexto = "#c7d19e";

                    row.RelativeItem().AlignCenter().Text(text =>
                    {
                        text.Span("✉ ").FontColor(colorTexto).FontSize(11);
                        text.Span("direccion@icgb.com.ar").FontColor(colorTexto).FontSize(9);
                    });

                    row.RelativeItem().AlignCenter().Text(text =>
                    {
                        text.Span("✉ ").FontColor(colorTexto).FontSize(11);
                        text.Span("secretaria.academica@icgb.com.ar").FontColor(colorTexto).FontSize(9);
                    });

                    row.RelativeItem().AlignCenter().Text(text =>
                    {
                        text.Span("✉ ").FontColor(colorTexto).FontSize(11);
                        text.Span("preceptoria@icgb.com.ar").FontColor(colorTexto).FontSize(9);
                    });
                });


            });
        });


    }
    private static byte[] ObtenerEncabezado()
    {
        string rutaEncabezado = Path.Combine(
        Directory.GetCurrentDirectory(),
        "wwwroot",
        "images",
        "encabezadoFormatoPrograma.JPG");

        return File.ReadAllBytes(rutaEncabezado);
    }
}
