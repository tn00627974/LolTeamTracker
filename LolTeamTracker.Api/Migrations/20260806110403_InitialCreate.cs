using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LolTeamTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Puuid = table.Column<string>(type: "nvarchar(78)", maxLength: 78, nullable: true),
                    GameName = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    TagLine = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Players_GameName_TagLine",
                table: "Players",
                columns: new[] { "GameName", "TagLine" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_Puuid",
                table: "Players",
                column: "Puuid",
                unique: true,
                filter: "[Puuid] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
