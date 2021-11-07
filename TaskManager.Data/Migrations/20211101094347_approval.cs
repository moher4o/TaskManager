using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Data.Migrations
{
    public partial class approval : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Approved",
                table: "WorkedHours",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                table: "WorkedHours",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkedHours_ApprovedBy",
                table: "WorkedHours",
                column: "ApprovedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkedHours_Employees_ApprovedBy",
                table: "WorkedHours",
                column: "ApprovedBy",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkedHours_Employees_ApprovedBy",
                table: "WorkedHours");

            migrationBuilder.DropIndex(
                name: "IX_WorkedHours_ApprovedBy",
                table: "WorkedHours");

            migrationBuilder.DropColumn(
                name: "Approved",
                table: "WorkedHours");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "WorkedHours");
        }
    }
}
