using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGDXRobot2Cloud.API.Migrations
{
    /// <inheritdoc />
    public partial class updateEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flows_SignalEmitters_SignalEmitterId",
                table: "Flows");

            migrationBuilder.DropTable(
                name: "SignalEmitters");

            migrationBuilder.RenameColumn(
                name: "SignalEmitterId",
                table: "Flows",
                newName: "SystemComponentId");

            migrationBuilder.RenameIndex(
                name: "IX_Flows_SignalEmitterId",
                table: "Flows",
                newName: "IX_Flows_SystemComponentId");

            migrationBuilder.CreateTable(
                name: "SystemComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemComponents", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Flows_SystemComponents_SystemComponentId",
                table: "Flows",
                column: "SystemComponentId",
                principalTable: "SystemComponents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Flows_SystemComponents_SystemComponentId",
                table: "Flows");

            migrationBuilder.DropTable(
                name: "SystemComponents");

            migrationBuilder.RenameColumn(
                name: "SystemComponentId",
                table: "Flows",
                newName: "SignalEmitterId");

            migrationBuilder.RenameIndex(
                name: "IX_Flows_SystemComponentId",
                table: "Flows",
                newName: "IX_Flows_SignalEmitterId");

            migrationBuilder.CreateTable(
                name: "SignalEmitters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignalEmitters", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Flows_SignalEmitters_SignalEmitterId",
                table: "Flows",
                column: "SignalEmitterId",
                principalTable: "SignalEmitters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
