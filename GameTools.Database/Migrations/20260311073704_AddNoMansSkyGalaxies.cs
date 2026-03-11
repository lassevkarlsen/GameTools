using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameTools.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNoMansSkyGalaxies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "GalaxyNumber",
                table: "NoMansSkyPortalAddresses",
                newName: "GalaxyId");

            migrationBuilder.CreateTable(
                name: "NoMansSkyGalaxies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoMansSkyGalaxies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NoMansSkyPortalAddresses_GalaxyId",
                table: "NoMansSkyPortalAddresses",
                column: "GalaxyId");

            migrationBuilder.AddForeignKey(
                name: "FK_NoMansSkyPortalAddresses_NoMansSkyGalaxies_GalaxyId",
                table: "NoMansSkyPortalAddresses",
                column: "GalaxyId",
                principalTable: "NoMansSkyGalaxies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NoMansSkyPortalAddresses_NoMansSkyGalaxies_GalaxyId",
                table: "NoMansSkyPortalAddresses");

            migrationBuilder.DropTable(
                name: "NoMansSkyGalaxies");

            migrationBuilder.DropIndex(
                name: "IX_NoMansSkyPortalAddresses_GalaxyId",
                table: "NoMansSkyPortalAddresses");

            migrationBuilder.RenameColumn(
                name: "GalaxyId",
                table: "NoMansSkyPortalAddresses",
                newName: "GalaxyNumber");
        }
    }
}
