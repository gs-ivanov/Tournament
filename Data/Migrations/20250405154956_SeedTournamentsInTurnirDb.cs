using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tournament.Data.Migrations
{
    public partial class SeedTournamentsInTurnirDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Teams_Tournaments_TournamentId",
                table: "Teams");

            migrationBuilder.DropIndex(
                name: "IX_Teams_TournamentId",
                table: "Teams");

            migrationBuilder.DropColumn(
                name: "TournamentId",
                table: "Teams");

            migrationBuilder.AlterColumn<string>(
                name: "Type",
                table: "Tournaments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.InsertData(
                table: "Tournaments",
                columns: new[] { "Id", "IsOpenForApplications", "Name", "StartDate", "Type" },
                values: new object[] { 1, true, "Пролетен турнир", new DateTime(2025, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Елиминации" });

            migrationBuilder.InsertData(
                table: "Tournaments",
                columns: new[] { "Id", "IsOpenForApplications", "Name", "StartDate", "Type" },
                values: new object[] { 2, true, "Летен шампионат", new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Групова фаза" });

            migrationBuilder.InsertData(
                table: "Tournaments",
                columns: new[] { "Id", "IsOpenForApplications", "Name", "StartDate", "Type" },
                values: new object[] { 3, false, "Зимна купа", new DateTime(2025, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Елиминации" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Tournaments",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Tournaments",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Tournaments",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.AlterColumn<int>(
                name: "Type",
                table: "Tournaments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TournamentId",
                table: "Teams",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Teams_TournamentId",
                table: "Teams",
                column: "TournamentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Teams_Tournaments_TournamentId",
                table: "Teams",
                column: "TournamentId",
                principalTable: "Tournaments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
