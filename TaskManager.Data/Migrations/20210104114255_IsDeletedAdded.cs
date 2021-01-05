using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Data.Migrations
{
    public partial class IsDeletedAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "WorkedHours",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "TasksTypes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "TasksStatuses",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Tasks",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Priorities",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Notes",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "JobTitles",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Employees",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "WorkedHours");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "TasksTypes");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "TasksStatuses");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Priorities");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Notes");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "JobTitles");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Employees");
        }
    }
}
