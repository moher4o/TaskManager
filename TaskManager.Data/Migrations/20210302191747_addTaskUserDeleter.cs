using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Data.Migrations
{
    public partial class addTaskUserDeleter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeletedByUserId",
                table: "Tasks",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_DeletedByUserId",
                table: "Tasks",
                column: "DeletedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Employees_DeletedByUserId",
                table: "Tasks",
                column: "DeletedByUserId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Employees_DeletedByUserId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_DeletedByUserId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Tasks");
        }
    }
}
