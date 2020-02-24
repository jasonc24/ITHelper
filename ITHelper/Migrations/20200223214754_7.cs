using Microsoft.EntityFrameworkCore.Migrations;

namespace ITHelper.Migrations
{
    public partial class _7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Update_Ticket_TicketId",
                table: "Update");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Update",
                table: "Update");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Ticket",
                table: "Ticket");

            migrationBuilder.RenameTable(
                name: "Update",
                newName: "Updates");

            migrationBuilder.RenameTable(
                name: "Ticket",
                newName: "Tickets");

            migrationBuilder.RenameIndex(
                name: "IX_Update_TicketId",
                table: "Updates",
                newName: "IX_Updates_TicketId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Updates",
                table: "Updates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tickets",
                table: "Tickets",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Updates_Tickets_TicketId",
                table: "Updates",
                column: "TicketId",
                principalTable: "Tickets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Updates_Tickets_TicketId",
                table: "Updates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Updates",
                table: "Updates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tickets",
                table: "Tickets");

            migrationBuilder.RenameTable(
                name: "Updates",
                newName: "Update");

            migrationBuilder.RenameTable(
                name: "Tickets",
                newName: "Ticket");

            migrationBuilder.RenameIndex(
                name: "IX_Updates_TicketId",
                table: "Update",
                newName: "IX_Update_TicketId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Update",
                table: "Update",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Ticket",
                table: "Ticket",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Update_Ticket_TicketId",
                table: "Update",
                column: "TicketId",
                principalTable: "Ticket",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
