using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoGestionAPI.Migrations
{
    /// <inheritdoc />
    public partial class MesaExamenRol : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "rol",
                table: "mesa_examen",
                type: "varchar(20)",
                unicode: false,
                maxLength: 20,
                nullable: false,
                defaultValue: "Presidente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "rol",
                table: "mesa_examen");
        }
    }
}
