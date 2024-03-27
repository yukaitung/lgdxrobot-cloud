using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGDXRobot2Cloud.API.Migrations
{
    /// <inheritdoc />
    public partial class dbFieldUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_NodesCompositions_NodesCompositionId",
                table: "Nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Robots_NodesCompositions_NodesCompositionId",
                table: "Robots");

            migrationBuilder.DropTable(
                name: "NodesCompositions");

            migrationBuilder.DropTable(
                name: "TaskWaypoint");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "Waypoints",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "Triggers",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "NodesCompositionId",
                table: "Robots",
                newName: "NodesCollectionId");

            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "Robots",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Robots_NodesCompositionId",
                table: "Robots",
                newName: "IX_Robots_NodesCollectionId");

            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "Progresses",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "NodesCompositionId",
                table: "Nodes",
                newName: "NodesCollectionId");

            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "Nodes",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_Nodes_NodesCompositionId",
                table: "Nodes",
                newName: "IX_Nodes_NodesCollectionId");

            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "Flows",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "ApiKeys",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "CreateAt",
                table: "ApiKeyLocations",
                newName: "CreatedAt");

            migrationBuilder.CreateTable(
                name: "NodesCollections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodesCollections", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RobotTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    FlowId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RobotTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RobotTasks_Flows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RobotTaskWaypoint",
                columns: table => new
                {
                    RobotTasksId = table.Column<int>(type: "int", nullable: false),
                    WaypointsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RobotTaskWaypoint", x => new { x.RobotTasksId, x.WaypointsId });
                    table.ForeignKey(
                        name: "FK_RobotTaskWaypoint_RobotTasks_RobotTasksId",
                        column: x => x.RobotTasksId,
                        principalTable: "RobotTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RobotTaskWaypoint_Waypoints_WaypointsId",
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
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6010), new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6010) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6010), new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6010) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6010), new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6010) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6010), new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6010) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6020), new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6020) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6020), new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6020) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6020), new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6020) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6020), new DateTime(2024, 3, 27, 19, 45, 52, 129, DateTimeKind.Utc).AddTicks(6020) });

            migrationBuilder.CreateIndex(
                name: "IX_RobotTasks_FlowId",
                table: "RobotTasks",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_RobotTaskWaypoint_WaypointsId",
                table: "RobotTaskWaypoint",
                column: "WaypointsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_NodesCollections_NodesCollectionId",
                table: "Nodes",
                column: "NodesCollectionId",
                principalTable: "NodesCollections",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Robots_NodesCollections_NodesCollectionId",
                table: "Robots",
                column: "NodesCollectionId",
                principalTable: "NodesCollections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nodes_NodesCollections_NodesCollectionId",
                table: "Nodes");

            migrationBuilder.DropForeignKey(
                name: "FK_Robots_NodesCollections_NodesCollectionId",
                table: "Robots");

            migrationBuilder.DropTable(
                name: "NodesCollections");

            migrationBuilder.DropTable(
                name: "RobotTaskWaypoint");

            migrationBuilder.DropTable(
                name: "RobotTasks");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Waypoints",
                newName: "CreateAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Triggers",
                newName: "CreateAt");

            migrationBuilder.RenameColumn(
                name: "NodesCollectionId",
                table: "Robots",
                newName: "NodesCompositionId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Robots",
                newName: "CreateAt");

            migrationBuilder.RenameIndex(
                name: "IX_Robots_NodesCollectionId",
                table: "Robots",
                newName: "IX_Robots_NodesCompositionId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Progresses",
                newName: "CreateAt");

            migrationBuilder.RenameColumn(
                name: "NodesCollectionId",
                table: "Nodes",
                newName: "NodesCompositionId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Nodes",
                newName: "CreateAt");

            migrationBuilder.RenameIndex(
                name: "IX_Nodes_NodesCollectionId",
                table: "Nodes",
                newName: "IX_Nodes_NodesCompositionId");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Flows",
                newName: "CreateAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ApiKeys",
                newName: "CreateAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "ApiKeyLocations",
                newName: "CreateAt");

            migrationBuilder.CreateTable(
                name: "NodesCompositions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CreateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NodesCompositions", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FlowId = table.Column<int>(type: "int", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tasks_Flows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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
                name: "IX_Tasks_FlowId",
                table: "Tasks",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_TaskWaypoint_WaypointsId",
                table: "TaskWaypoint",
                column: "WaypointsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Nodes_NodesCompositions_NodesCompositionId",
                table: "Nodes",
                column: "NodesCompositionId",
                principalTable: "NodesCompositions",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Robots_NodesCompositions_NodesCompositionId",
                table: "Robots",
                column: "NodesCompositionId",
                principalTable: "NodesCompositions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
