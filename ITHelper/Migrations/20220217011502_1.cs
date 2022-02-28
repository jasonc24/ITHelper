using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelper.Migrations
{
    public partial class _1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AVTicketId",
                table: "Updates",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AVTickets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EMail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetMinistry = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LocationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneralIdea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExistingFootage = table.Column<bool>(type: "bit", nullable: false),
                    StoryBoards = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DateSubmitted = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastUpdated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AVTickets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AVTickets_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AVTickets_Locations_LocationId",
                        column: x => x.LocationId,
                        principalTable: "Locations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Updates_AVTicketId",
                table: "Updates",
                column: "AVTicketId");

            migrationBuilder.CreateIndex(
                name: "IX_AVTickets_CategoryId",
                table: "AVTickets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AVTickets_LocationId",
                table: "AVTickets",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Updates_AVTickets_AVTicketId",
                table: "Updates",
                column: "AVTicketId",
                principalTable: "AVTickets",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Updates_AVTickets_AVTicketId",
                table: "Updates");

            migrationBuilder.DropTable(
                name: "AVTickets");

            migrationBuilder.DropIndex(
                name: "IX_Updates_AVTicketId",
                table: "Updates");

            migrationBuilder.DropColumn(
                name: "AVTicketId",
                table: "Updates");
        }
    }
}
