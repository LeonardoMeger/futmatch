using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FutMatchApp.Migrations
{
    /// <inheritdoc />
    public partial class AddGooglePlacesIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GooglePlaceId",
                table: "Courts",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "GoogleRating",
                table: "Courts",
                type: "decimal(3,2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFromGoogle",
                table: "Courts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Latitude",
                table: "Courts",
                type: "decimal(10,8)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Longitude",
                table: "Courts",
                type: "decimal(11,8)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Courts",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "GooglePlaceId", "GoogleRating", "IsFromGoogle", "Latitude", "Longitude" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Courts",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "GooglePlaceId", "GoogleRating", "IsFromGoogle", "Latitude", "Longitude" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.UpdateData(
                table: "Courts",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "GooglePlaceId", "GoogleRating", "IsFromGoogle", "Latitude", "Longitude" },
                values: new object[] { null, null, false, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Courts_GooglePlaceId",
                table: "Courts",
                column: "GooglePlaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Courts_GooglePlaceId",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "GooglePlaceId",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "GoogleRating",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "IsFromGoogle",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Courts");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Courts");
        }
    }
}
