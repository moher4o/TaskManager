using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TaskManager.Data.Migrations
{
    public partial class mobMessages : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MessageDate = table.Column<DateTime>(nullable: false),
                    Text = table.Column<string>(maxLength: 350, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MessagesParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderId = table.Column<int>(nullable: false),
                    ReceiverId = table.Column<int>(nullable: false),
                    MessageId = table.Column<int>(nullable: false),
                    isReceived = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessagesParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MessagesParticipants_Messages_MessageId",
                        column: x => x.MessageId,
                        principalTable: "Messages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MessagesParticipants_Employees_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MessagesParticipants_Employees_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MessagesParticipants_MessageId",
                table: "MessagesParticipants",
                column: "MessageId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesParticipants_ReceiverId",
                table: "MessagesParticipants",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_MessagesParticipants_SenderId",
                table: "MessagesParticipants",
                column: "SenderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MessagesParticipants");

            migrationBuilder.DropTable(
                name: "Messages");
        }
    }
}
