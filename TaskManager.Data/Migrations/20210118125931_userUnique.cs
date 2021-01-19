using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Data.Migrations
{
    public partial class userUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Employees_DaeuAccaunt",
                table: "Employees",
                column: "DaeuAccaunt",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Directorates_Name",
                table: "Directorates",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_DaeuAccaunt",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Directorates_Name",
                table: "Directorates");
        }
    }
}
