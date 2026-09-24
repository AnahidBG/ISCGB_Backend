using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AutoGestionAPI.Services
{
    public static class GeneradorPDFCertificado
    {
        public static byte[] CrearCertificado(
            string nombreCompleto,
            string dni,
            bool conHorario)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            DateTime fecha = DateTime.Now;

            byte[] sello = ObtenerSello();

            string mes = ObtenerMes(fecha.Month);

            string fechaTexto =
                $"Se expide la presente en Capital a los {fecha.Day} días del mes de {mes} de {fecha.Year}.";

            IDocument documento = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    page.Margin(2, Unit.Centimetre);

                    page.PageColor(Colors.White);

                    page.DefaultTextStyle(text =>
                        text.FontSize(12)
                            .FontFamily(Fonts.Arial));

                    page.Content()
                        .Column(col =>
                        {
                            col.Spacing(15);

                            // TÍTULO
                            col.Item()
                                .AlignCenter()
                                .Text("CONSTANCIA DE ALUMNO REGULAR")
                                .Bold()
                                .FontSize(16);

                            // INSTITUTO
                            col.Item()
                                .PaddingTop(20)
                                .Text(
                                    "La dirección del Instituto Superior Cura Gabriel Brochero");

                            // NOMBRE
                            col.Item()
                                .PaddingTop(10)
                                .Text(text =>
                                {
                                    text.Span("HACE CONSTAR que: ")
                                        .Bold();

                                    text.Span(nombreCompleto);
                                });

                            // DNI
                            col.Item()
                                .Text(text =>
                                {
                                    text.Span("Documento: ")
                                        .Bold();

                                    text.Span($"D.N.I. {dni}");
                                });

                            // CARRERA
                            col.Item()
                                .PaddingTop(10)
                                .Text(text =>
                                {
                                    text.Span(
                                        "Es alumno regular de la carrera ");

                                    text.Span(
                                        "TECNICATURA SUPERIOR EN DESARROLLO DE SOFTWARE")
                                        .Bold();
                                });

                            // HORARIOS
                            if (conHorario)
                            {
                                col.Item()
                                    .PaddingTop(10)
                                    .Text("Cursa en los siguientes horarios:")
                                    .Bold();

                                AgregarHorario(col, "Lunes");

                                AgregarHorario(col, "Martes");

                                AgregarHorario(col, "Miércoles");

                                AgregarHorario(col, "Jueves");

                                AgregarHorario(col, "Viernes");
                            }

                            // TEXTO FINAL
                            col.Item()
                                .PaddingTop(15)
                                .Text(
                                    "A pedido del Interesado y al solo efecto de ser presentado.");

                            // FECHA
                            col.Item()
                                .PaddingTop(50)
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                    .AlignCenter()
                                    .Width(120)
                                    .Image(sello);

                                row.RelativeItem()
                                    .AlignRight()
                                    .AlignMiddle()
                                    .Text("Código de Validación: XXXXX")
                                    .FontSize(10);
                            });
                        });
                });
            });

            return documento.GeneratePdf();
        }

        //SELLO
        private static byte[] ObtenerSello()
        {
            string rutaSello = Path.Combine(
            AppContext.BaseDirectory,
            "wwwroot",
            "images",
            "sello.png");

            return File.ReadAllBytes(rutaSello);
        }


        private static void AgregarHorario(
            ColumnDescriptor col,
            string dia)
        {
            col.Item()
                .Text(
                    $"{dia} de ____________________ a ____________________");
        }

        private static string ObtenerMes(int numeroMes)
        {
            string[] meses =
            {
                "enero",
                "febrero",
                "marzo",
                "abril",
                "mayo",
                "junio",
                "julio",
                "agosto",
                "septiembre",
                "octubre",
                "noviembre",
                "diciembre"
            };

            return meses[numeroMes - 1];
        }


    }
}