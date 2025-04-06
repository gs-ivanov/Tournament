using Microsoft.EntityFrameworkCore.Migrations;

namespace Tournament.Data.Migrations
{
    public partial class AddFieldsToManagerRequestAndTournament : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOpenForApplications",
                table: "Tournaments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "JsonPayload",
                table: "ManagerRequests",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "FeePaid",
                table: "ManagerRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "ManagerRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TournamentId",
                table: "ManagerRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ManagerRequests_TournamentId",
                table: "ManagerRequests",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_ManagerRequests_Tournaments_TournamentId",
                table: "ManagerRequests",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ManagerRequests_Tournaments_TournamentId",
                table: "ManagerRequests");

            migrationBuilder.DropIndex(
                name: "IX_ManagerRequests_TournamentId",
                table: "ManagerRequests");

            migrationBuilder.DropColumn(
                name: "IsOpenForApplications",
                table: "Tournaments");

            migrationBuilder.DropColumn(
                name: "FeePaid",
                table: "ManagerRequests");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "ManagerRequests");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "ManagerRequests");

            migrationBuilder.AlterColumn<string>(
                name: "JsonPayload",
                table: "ManagerRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
