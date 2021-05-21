using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Data.Migrations
{
    public partial class representative : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RepresentativeId",
                table: "Employees",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_RepresentativeId",
                table: "Employees",
                column: "RepresentativeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_RepresentativeId",
                table: "Employees",
                column: "RepresentativeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_RepresentativeId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_RepresentativeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "RepresentativeId",
                table: "Employees");
        }
    }
}
