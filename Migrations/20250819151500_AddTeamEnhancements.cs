using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutMatchApp.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CorPrimaria",
                table: "Teams",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorSecundaria",
                table: "Teams",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "Teams",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdadeMaxima",
                table: "Teams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IdadeMinima",
                table: "Teams",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorPrimaria",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "CorSecundaria",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "IdadeMaxima",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "IdadeMinima",
                table: "Teams");
        }
    }
}
