using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AutoGestionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregaSuplenciasDocente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AddColumn<bool>(
                name: "es_suplente",
                table: "Docentes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_fin_suplencia",
                table: "Docentes",
                type: "datetime",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_inicio_suplencia",
                table: "Docentes",
                type: "datetime",
                nullable: true);


        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "id_rol",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "es_suplente",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "fecha_fin_suplencia",
                table: "Docentes");

            migrationBuilder.DropColumn(
                name: "fecha_inicio_suplencia",
                table: "Docentes");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "id_rol",
                keyValue: 2,
                column: "rol",
                value: "Docente");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "id_rol",
                keyValue: 3,
                column: "rol",
                value: "Alumno");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "id_rol",
                keyValue: 4,
                column: "rol",
                value: "Secretario");

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "id_rol", "rol" },
                values: new object[] { 5, "Director" });

            migrationBuilder.InsertData(
                table: "roles_tipos_documentos",
                columns: new[] { "id_roles_tipos_documentos", "anual", "id_rol", "id_tipo_doc", "obligatorio" },
                values: new object[,]
                {
                    { 13, false, 4, 10, false },
                    { 14, false, 4, 11, false }
                });
        }
    }
}
