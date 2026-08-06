using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LolTeamTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class MakePuuidRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_Puuid",
                table: "Players");

            migrationBuilder.AlterColumn<string>(
                name: "Puuid",
                table: "Players",
                type: "nvarchar(78)",
                maxLength: 78,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(78)",
                oldMaxLength: 78,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_Puuid",
                table: "Players",
                column: "Puuid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Players_Puuid",
                table: "Players");

            migrationBuilder.AlterColumn<string>(
                name: "Puuid",
                table: "Players",
                type: "nvarchar(78)",
                maxLength: 78,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(78)",
                oldMaxLength: 78);

            migrationBuilder.CreateIndex(
                name: "IX_Players_Puuid",
                table: "Players",
                column: "Puuid",
                unique: true,
                filter: "[Puuid] IS NOT NULL");
        }
    }
}
