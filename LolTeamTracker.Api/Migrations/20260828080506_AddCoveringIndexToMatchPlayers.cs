using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LolTeamTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCoveringIndexToMatchPlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MatchPlayers_PlayerId_GameDate",
                table: "MatchPlayers");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPlayers_PlayerId_GameDate",
                table: "MatchPlayers",
                columns: new[] { "PlayerId", "GameDate" })
                .Annotation("SqlServer:Include", new[] { "MatchId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MatchPlayers_PlayerId_GameDate",
                table: "MatchPlayers");

            migrationBuilder.CreateIndex(
                name: "IX_MatchPlayers_PlayerId_GameDate",
                table: "MatchPlayers",
                columns: new[] { "PlayerId", "GameDate" });
        }
    }
}
