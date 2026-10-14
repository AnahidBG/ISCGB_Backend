using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoGestionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregarNotificacionesAutomatizadas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_ultima_notificacion",
                table: "Usuarios",
                type: "datetime2",
                nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "Formato",
            //     table: "materias",
            //     type: "nvarchar(max)",
            //     nullable: true);

            // migrationBuilder.AddColumn<int>(
            //     name: "HorasCatedra",
            //     table: "materias",
            //     type: "int",
            //     nullable: true);

            // migrationBuilder.AddColumn<int>(
            //     name: "HorasTotales",
            //     table: "materias",
            //     type: "int",
            //     nullable: true);

            // migrationBuilder.AddColumn<int>(
            //     name: "NroOrden",
            //     table: "materias",
            //     type: "int",
            //     nullable: true);

            // migrationBuilder.AddColumn<string>(
            //     name: "Legajo",
            //     table: "Alumnos",
            //     type: "nvarchar(max)",
            //     nullable: true);

            migrationBuilder.CreateTable(
                name: "configuracion_sistema",
                columns: table => new
                {
                    id_configuracion = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    frecuencia_notificacion_dias = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracion_sistema", x => x.id_configuracion);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "configuracion_sistema");

            migrationBuilder.DropColumn(
                name: "fecha_ultima_notificacion",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Formato",
                table: "materias");

            migrationBuilder.DropColumn(
                name: "HorasCatedra",
                table: "materias");

            migrationBuilder.DropColumn(
                name: "HorasTotales",
                table: "materias");

            migrationBuilder.DropColumn(
                name: "NroOrden",
                table: "materias");

            migrationBuilder.DropColumn(
                name: "Legajo",
                table: "Alumnos");
        }
    }
}
