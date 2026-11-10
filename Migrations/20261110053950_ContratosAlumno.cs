using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoGestionAPI.Migrations
{
    /// <inheritdoc />
    public partial class ContratosAlumno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contratos_alumno",
                columns: table => new
                {
                    id_contrato = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_usuario = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ruta_archivo = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    fecha_carga = table.Column<DateTime>(type: "datetime", nullable: false),
                    id_usuario_carga = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contratos_alumno", x => x.id_contrato);
                    table.ForeignKey(
                        name: "FK_ContratosAlumno_Usuario",
                        column: x => x.id_usuario,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario");
                    table.ForeignKey(
                        name: "FK_ContratosAlumno_UsuarioCarga",
                        column: x => x.id_usuario_carga,
                        principalTable: "Usuarios",
                        principalColumn: "id_usuario");
                });

            migrationBuilder.CreateIndex(
                name: "IX_contratos_alumno_id_usuario",
                table: "contratos_alumno",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_contratos_alumno_id_usuario_carga",
                table: "contratos_alumno",
                column: "id_usuario_carga");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "contratos_alumno");
        }
    }
}
