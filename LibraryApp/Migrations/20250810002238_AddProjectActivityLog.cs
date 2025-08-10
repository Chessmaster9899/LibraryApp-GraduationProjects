using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryApp.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectActivityLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProjectActivityLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<int>(type: "INTEGER", nullable: false),
                    Activity = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Details = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    PerformedBy = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectActivityLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProjectActivityLogs_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectActivityLogs_ProjectId",
                table: "ProjectActivityLogs",
                column: "ProjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProjectActivityLogs");
        }
    }
}
