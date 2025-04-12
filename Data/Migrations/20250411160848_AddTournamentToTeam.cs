using Microsoft.EntityFrameworkCore.Migrations;

namespace Tournament.Data.Migrations
{
    public partial class AddTournamentToTeam : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TournamentId",
                table: "Teams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TournamentId1",
                table: "Teams",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_TournamentId",
                table: "Teams",
                column: "TournamentId");

            migrationBuilder.CreateIndex(
                name: "IX_Teams_TournamentId1",
                table: "Teams",
                column: "TournamentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Tournaments_TournamentId",
                table: "Teams",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Tournaments_TournamentId1",
                table: "Teams",
                column: "TournamentId1",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Tournaments_TournamentId",
                table: "Teams");

            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Tournaments_TournamentId1",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_TournamentId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_TournamentId1",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "TournamentId1",
                table: "Teams");
        }
    }
}
