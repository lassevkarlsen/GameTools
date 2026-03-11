using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GameTools.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDbContextProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NoMansSkyGuildSystems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Guild = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoMansSkyGuildSystems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoMansSkyGuildSystems_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NoMansSkyGuildSystemRewards",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildSystemId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastRedeemed = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoMansSkyGuildSystemRewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NoMansSkyGuildSystemRewards_NoMansSkyGuildSystems_GuildSyst~",
                        column: x => x.GuildSystemId,
                        principalTable: "NoMansSkyGuildSystems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NoMansSkyGuildSystemRewards_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NoMansSkyGuildSystemRewards_GuildSystemId",
                table: "NoMansSkyGuildSystemRewards",
                column: "GuildSystemId");

            migrationBuilder.CreateIndex(
                name: "IX_NoMansSkyGuildSystemRewards_ProfileId",
                table: "NoMansSkyGuildSystemRewards",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_NoMansSkyGuildSystems_ProfileId",
                table: "NoMansSkyGuildSystems",
                column: "ProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NoMansSkyGuildSystemRewards");

            migrationBuilder.DropTable(
                name: "NoMansSkyGuildSystems");
        }
    }
}
