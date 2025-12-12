using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameTools.Database.Migrations
{
    /// <inheritdoc />
    public partial class Renameprofiletable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameTimers_ProfileSettings_ProfileId",
                table: "GameTimers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProfileSettings",
                table: "ProfileSettings");

            migrationBuilder.RenameTable(
                name: "ProfileSettings",
                newName: "Profiles");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Profiles",
                table: "Profiles",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameTimers_Profiles_ProfileId",
                table: "GameTimers",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GameTimers_Profiles_ProfileId",
                table: "GameTimers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Profiles",
                table: "Profiles");

            migrationBuilder.RenameTable(
                name: "Profiles",
                newName: "ProfileSettings");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProfileSettings",
                table: "ProfileSettings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_GameTimers_ProfileSettings_ProfileId",
                table: "GameTimers",
                column: "ProfileId",
                principalTable: "ProfileSettings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
