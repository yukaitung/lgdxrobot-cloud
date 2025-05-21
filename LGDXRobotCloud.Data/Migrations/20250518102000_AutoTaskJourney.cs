using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LGDXRobotCloud.Data.Migrations
{
    /// <inheritdoc />
    public partial class AutoTaskJourney : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Automation.AutoTaskJourney",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CurrentProgressId = table.Column<int>(type: "integer", nullable: true),
                    AutoTaskId = table.Column<int>(type: "integer", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automation.AutoTaskJourney", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Automation.AutoTaskJourney_Automation.AutoTasks_AutoTaskId",
                        column: x => x.AutoTaskId,
                        principalTable: "Automation.AutoTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Automation.AutoTaskJourney_Automation.Progresses_CurrentPro~",
                        column: x => x.CurrentProgressId,
                        principalTable: "Automation.Progresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Automation.AutoTaskJourney_AutoTaskId",
                table: "Automation.AutoTaskJourney",
                column: "AutoTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.AutoTaskJourney_CurrentProgressId",
                table: "Automation.AutoTaskJourney",
                column: "CurrentProgressId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Automation.AutoTaskJourney");
        }
    }
}
