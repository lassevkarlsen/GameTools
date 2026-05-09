using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameTools.Database.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNotificationSentProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationSent",
                table: "Diablo4EventNotifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "NotificationSent",
                table: "Diablo4EventNotifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
