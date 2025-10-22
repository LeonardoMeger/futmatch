using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutMatchApp.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResultadoInformado",
                table: "Reservations",
                newName: "ResultadoInformadoTime2");

            migrationBuilder.RenameColumn(
                name: "ResultadoConfirmado",
                table: "Reservations",
                newName: "ResultadoInformadoTime1");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataResultadoFinal",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataResultadoTime1",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataResultadoTime2",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GolsTime1Final",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GolsTime1_InformadoPeloTime1",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GolsTime1_InformadoPeloTime2",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GolsTime2Final",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GolsTime2_InformadoPeloTime1",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GolsTime2_InformadoPeloTime2",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResultadoTime1Final",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResultadoTime2Final",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusResultado",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataResultadoFinal",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "DataResultadoTime1",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "DataResultadoTime2",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "GolsTime1Final",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "GolsTime1_InformadoPeloTime1",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "GolsTime1_InformadoPeloTime2",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "GolsTime2Final",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "GolsTime2_InformadoPeloTime1",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "GolsTime2_InformadoPeloTime2",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ResultadoTime1Final",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ResultadoTime2Final",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "StatusResultado",
                table: "Reservations");

            migrationBuilder.RenameColumn(
                name: "ResultadoInformadoTime2",
                table: "Reservations",
                newName: "ResultadoInformado");

            migrationBuilder.RenameColumn(
                name: "ResultadoInformadoTime1",
                table: "Reservations",
                newName: "ResultadoConfirmado");
        }
    }
}
