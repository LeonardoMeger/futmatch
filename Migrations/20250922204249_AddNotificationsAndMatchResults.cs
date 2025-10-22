using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutMatchApp.Migrations
{
    /// <inheritdoc />
    public partial class AddNotificationsAndMatchResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataResultado",
                table: "Reservations",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GolsTime1",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GolsTime2",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ObservacoesPartida",
                table: "Reservations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ResultadoConfirmado",
                table: "Reservations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ResultadoTime1",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ResultadoTime2",
                table: "Reservations",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcaoNegativa",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcaoPositiva",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PermiteAcao",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "UrlAcao",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataResultado",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "GolsTime1",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "GolsTime2",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ObservacoesPartida",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ResultadoConfirmado",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ResultadoTime1",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "ResultadoTime2",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "AcaoNegativa",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "AcaoPositiva",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "PermiteAcao",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "UrlAcao",
                table: "Notifications");
        }
    }
}
