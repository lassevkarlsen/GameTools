using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameTools.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreNoMansSkyPortalAddressData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CoordinatesX",
                table: "NoMansSkyPortalAddresses",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CoordinatesY",
                table: "NoMansSkyPortalAddresses",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlanetName",
                table: "NoMansSkyPortalAddresses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SystemName",
                table: "NoMansSkyPortalAddresses",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CoordinatesX",
                table: "NoMansSkyPortalAddresses");

            migrationBuilder.DropColumn(
                name: "CoordinatesY",
                table: "NoMansSkyPortalAddresses");

            migrationBuilder.DropColumn(
                name: "PlanetName",
                table: "NoMansSkyPortalAddresses");

            migrationBuilder.DropColumn(
                name: "SystemName",
                table: "NoMansSkyPortalAddresses");
        }
    }
}
