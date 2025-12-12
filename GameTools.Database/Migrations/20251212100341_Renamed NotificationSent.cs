using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameTools.Database.Migrations
{
    /// <inheritdoc />
    public partial class RenamedNotificationSent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NotificationSent",
                table: "GameTimers",
                newName: "CompletionProcessed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CompletionProcessed",
                table: "GameTimers",
                newName: "NotificationSent");
        }
    }
}
