using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameTools.Database.Migrations
{
    /// <inheritdoc />
    public partial class CleanupDiablo4Notifications : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Diablo4EventNotifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Diablo4EventNotifications");

            migrationBuilder.AlterColumn<string>(
                name: "EventText",
                table: "Diablo4EventNotifications",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Key",
                table: "Diablo4EventNotifications",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Key",
                table: "Diablo4EventNotifications");

            migrationBuilder.AlterColumn<string>(
                name: "EventText",
                table: "Diablo4EventNotifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.AddColumn<string>(
                name: "EventId",
                table: "Diablo4EventNotifications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Diablo4EventNotifications",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
