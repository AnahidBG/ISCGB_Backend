using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AutoGestionAPI.Migrations
{
    /// <inheritdoc />
    public partial class CargaInicialRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            // migrationBuilder.InsertData(
            //     table: "Roles",
            //     columns: new[] { "id_rol", "rol" },
            //     values: new object[,]
            //     {
            //         { 2, "Docente" },
            //         { 3, "Alumno" },
            //         { 4, "Secretario" },
            //         { 5, "Director" }
            //     });

            // migrationBuilder.InsertData(
            //     table: "tipos_documentos",
            //     columns: new[] { "id_tipo_doc", "nombre_documento" },
            //     values: new object[,]
            //     {
            //         { 1, "DNI" },
            //         { 2, "Curriculum Vitae" },
            //         { 3, "Titulo Profesional" },
            //         { 4, "DDJJ Incompatibilidad Horaria" },
            //         { 5, "Apto Medico" },
            //         { 6, "Apto Psicologico" },
            //         { 7, "Antecedentes Penales" },
            //         { 8, "Ley 9680" },
            //         { 9, "Certificado De Domicilio" }
            //     });

            // migrationBuilder.InsertData(
            //     table: "roles_tipos_documentos",
            //     columns: new[] { "id_roles_tipos_documentos", "anual", "id_rol", "id_tipo_doc", "obligatorio" },
            //     values: new object[,]
            //     {
            //         { 1, false, 3, 1, false },
            //         { 2, false, 3, 2, false },
            //         { 3, false, 3, 3, false },
            //         { 4, false, 3, 4, true },
            //         { 5, false, 3, 5, true },
            //         { 6, false, 3, 6, true },
            //         { 7, false, 3, 7, false },
            //         { 8, false, 3, 8, true },
            //         { 9, false, 3, 9, true },
            //         { 10, false, 4, 1, false },
            //         { 11, false, 4, 5, true },
            //         { 12, false, 4, 7, false }
            //     });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alumno_materia");

            migrationBuilder.DropTable(
                name: "contenidos");

            migrationBuilder.DropTable(
                name: "Correlatividade");

            migrationBuilder.DropTable(
                name: "docente_materia");

            migrationBuilder.DropTable(
                name: "docente_tipo_titulo");

            migrationBuilder.DropTable(
                name: "Justificativos");

            migrationBuilder.DropTable(
                name: "legajo");

            migrationBuilder.DropTable(
                name: "mesa_examen");

            migrationBuilder.DropTable(
                name: "PlanesMateria");

            migrationBuilder.DropTable(
                name: "reconocimiento_saberes");

            migrationBuilder.DropTable(
                name: "roles_tipos_documentos");

            migrationBuilder.DropTable(
                name: "Usuarios_roles");

            migrationBuilder.DropTable(
                name: "programas_materia");

            migrationBuilder.DropTable(
                name: "tipo_titulo");

            migrationBuilder.DropTable(
                name: "Examenes");

            migrationBuilder.DropTable(
                name: "PlanesEstudio");

            migrationBuilder.DropTable(
                name: "Alumnos");

            migrationBuilder.DropTable(
                name: "tipos_documentos");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Docentes");

            migrationBuilder.DropTable(
                name: "comision");

            migrationBuilder.DropTable(
                name: "materias");

            migrationBuilder.DropTable(
                name: "tipo_examen");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Provincia");

            migrationBuilder.DropTable(
                name: "Pais");
        }
    }
}
