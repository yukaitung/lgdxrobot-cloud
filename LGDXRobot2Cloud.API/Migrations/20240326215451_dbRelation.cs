using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGDXRobot2Cloud.API.Migrations
{
    /// <inheritdoc />
    public partial class dbRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Progresses_Flows_FlowId",
                table: "Progresses");

            migrationBuilder.DropForeignKey(
                name: "FK_Triggers_Flows_FlowId",
                table: "Triggers");

            migrationBuilder.DropForeignKey(
                name: "FK_Triggers_Flows_FlowId1",
                table: "Triggers");

            migrationBuilder.DropIndex(
                name: "IX_Triggers_FlowId",
                table: "Triggers");

            migrationBuilder.DropIndex(
                name: "IX_Triggers_FlowId1",
                table: "Triggers");

            migrationBuilder.DropIndex(
                name: "IX_Progresses_FlowId",
                table: "Progresses");

            migrationBuilder.DropColumn(
                name: "FlowId",
                table: "Triggers");

            migrationBuilder.DropColumn(
                name: "FlowId1",
                table: "Triggers");

            migrationBuilder.DropColumn(
                name: "FlowId",
                table: "Progresses");

            migrationBuilder.CreateTable(
                name: "FlowEndTrigger",
                columns: table => new
                {
                    EndTriggersId = table.Column<int>(type: "int", nullable: false),
                    FlowsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowEndTrigger", x => new { x.EndTriggersId, x.FlowsId });
                    table.ForeignKey(
                        name: "FK_FlowEndTrigger_Flows_FlowsId",
                        column: x => x.FlowsId,
                        principalTable: "Flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowEndTrigger_Triggers_EndTriggersId",
                        column: x => x.EndTriggersId,
                        principalTable: "Triggers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FlowProgress",
                columns: table => new
                {
                    FlowsId = table.Column<int>(type: "int", nullable: false),
                    ProgressesId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowProgress", x => new { x.FlowsId, x.ProgressesId });
                    table.ForeignKey(
                        name: "FK_FlowProgress_Flows_FlowsId",
                        column: x => x.FlowsId,
                        principalTable: "Flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowProgress_Progresses_ProgressesId",
                        column: x => x.ProgressesId,
                        principalTable: "Progresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FlowStartTrigger",
                columns: table => new
                {
                    EndTriggersId = table.Column<int>(type: "int", nullable: false),
                    FlowsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlowStartTrigger", x => new { x.EndTriggersId, x.FlowsId });
                    table.ForeignKey(
                        name: "FK_FlowStartTrigger_Flows_FlowsId",
                        column: x => x.FlowsId,
                        principalTable: "Flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FlowStartTrigger_Triggers_EndTriggersId",
                        column: x => x.EndTriggersId,
                        principalTable: "Triggers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8430), new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8430) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8440), new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8440) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8440), new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8440) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8440), new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8440) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8440), new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8440) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8440), new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8440) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8440), new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8440) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8450), new DateTime(2024, 3, 26, 21, 54, 51, 546, DateTimeKind.Utc).AddTicks(8450) });

            migrationBuilder.CreateIndex(
                name: "IX_FlowEndTrigger_FlowsId",
                table: "FlowEndTrigger",
                column: "FlowsId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowProgress_ProgressesId",
                table: "FlowProgress",
                column: "ProgressesId");

            migrationBuilder.CreateIndex(
                name: "IX_FlowStartTrigger_FlowsId",
                table: "FlowStartTrigger",
                column: "FlowsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlowEndTrigger");

            migrationBuilder.DropTable(
                name: "FlowProgress");

            migrationBuilder.DropTable(
                name: "FlowStartTrigger");

            migrationBuilder.AddColumn<int>(
                name: "FlowId",
                table: "Triggers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FlowId1",
                table: "Triggers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FlowId",
                table: "Progresses",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateAt", "FlowId", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6090), null, new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6090) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreateAt", "FlowId", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), null, new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreateAt", "FlowId", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), null, new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreateAt", "FlowId", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), null, new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreateAt", "FlowId", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), null, new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreateAt", "FlowId", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), null, new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreateAt", "FlowId", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), null, new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreateAt", "FlowId", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), null, new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_FlowId",
                table: "Triggers",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_FlowId1",
                table: "Triggers",
                column: "FlowId1");

            migrationBuilder.CreateIndex(
                name: "IX_Progresses_FlowId",
                table: "Progresses",
                column: "FlowId");

            migrationBuilder.AddForeignKey(
                name: "FK_Progresses_Flows_FlowId",
                table: "Progresses",
                column: "FlowId",
                principalTable: "Flows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Triggers_Flows_FlowId",
                table: "Triggers",
                column: "FlowId",
                principalTable: "Flows",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Triggers_Flows_FlowId1",
                table: "Triggers",
                column: "FlowId1",
                principalTable: "Flows",
                principalColumn: "Id");
        }
    }
}
