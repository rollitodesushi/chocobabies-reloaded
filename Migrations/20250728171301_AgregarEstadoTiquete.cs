using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChocobabiesReloaded.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoTiquete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "comentarios",
                table: "tiquetes",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "estado",
                table: "tiquetes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "comentarios",
                table: "tiquetes");

            migrationBuilder.DropColumn(
                name: "estado",
                table: "tiquetes");
        }
    }
}
