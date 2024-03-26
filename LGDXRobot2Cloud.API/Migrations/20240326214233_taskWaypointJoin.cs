using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGDXRobot2Cloud.API.Migrations
{
    /// <inheritdoc />
    public partial class taskWaypointJoin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Waypoints_Tasks_TaskId",
                table: "Waypoints");

            migrationBuilder.DropIndex(
                name: "IX_Waypoints_TaskId",
                table: "Waypoints");

            migrationBuilder.DropColumn(
                name: "TaskId",
                table: "Waypoints");

            migrationBuilder.CreateTable(
                name: "TaskWaypoint",
                columns: table => new
                {
                    TasksId = table.Column<int>(type: "int", nullable: false),
                    WaypointsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskWaypoint", x => new { x.TasksId, x.WaypointsId });
                    table.ForeignKey(
                        name: "FK_TaskWaypoint_Tasks_TasksId",
                        column: x => x.TasksId,
                        principalTable: "Tasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TaskWaypoint_Waypoints_WaypointsId",
                        column: x => x.WaypointsId,
                        principalTable: "Waypoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6090), new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6090) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100), new DateTime(2024, 3, 26, 21, 42, 32, 811, DateTimeKind.Utc).AddTicks(6100) });

            migrationBuilder.CreateIndex(
                name: "IX_TaskWaypoint_WaypointsId",
                table: "TaskWaypoint",
                column: "WaypointsId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TaskWaypoint");

            migrationBuilder.AddColumn<int>(
                name: "TaskId",
                table: "Waypoints",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250), new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250), new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250), new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250), new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260), new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260), new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260), new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreateAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260), new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260) });

            migrationBuilder.CreateIndex(
                name: "IX_Waypoints_TaskId",
                table: "Waypoints",
                column: "TaskId");

            migrationBuilder.AddForeignKey(
                name: "FK_Waypoints_Tasks_TaskId",
                table: "Waypoints",
                column: "TaskId",
                principalTable: "Tasks",
                principalColumn: "Id");
        }
    }
}
