using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Data.Migrations
{
    public partial class AddedUserWhoCloseTask : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CloseUserId",
                table: "Tasks",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_CloseUserId",
                table: "Tasks",
                column: "CloseUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Employees_CloseUserId",
                table: "Tasks",
                column: "CloseUserId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Employees_CloseUserId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_CloseUserId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CloseUserId",
                table: "Tasks");
        }
    }
}
