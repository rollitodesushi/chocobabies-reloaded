using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChocobabiesReloaded.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketPriceToRaffle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "fechaSorteo",
                table: "rifas",
                newName: "fechaInicioSorteo");

            migrationBuilder.AddColumn<DateTime>(
                name: "fechaCierreSorteo",
                table: "rifas",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fechaCierreSorteo",
                table: "rifas");

            migrationBuilder.RenameColumn(
                name: "fechaInicioSorteo",
                table: "rifas",
                newName: "fechaSorteo");
        }
    }
}
