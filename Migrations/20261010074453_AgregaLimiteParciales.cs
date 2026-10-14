using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoGestionAPI.Migrations
{
    /// <inheritdoc />
    public partial class AgregaLimiteParciales : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "limite_parciales_semana",
                table: "configuracion_sistema",
                type: "int",
                nullable: false,
                defaultValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "limite_parciales_semana",
                table: "configuracion_sistema");
        }
    }
}
