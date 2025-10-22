using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutMatchApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchResultFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ResultadoInformado",
                table: "Reservations",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResultadoInformado",
                table: "Reservations");
        }
    }
}
