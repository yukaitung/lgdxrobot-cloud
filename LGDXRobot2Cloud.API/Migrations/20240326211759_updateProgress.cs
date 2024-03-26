using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LGDXRobot2Cloud.API.Migrations
{
    /// <inheritdoc />
    public partial class updateProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "System",
                table: "Progresses",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "Progresses",
                columns: new[] { "Id", "CreateAt", "FlowId", "Name", "System", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250), null, "Waiting", true, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250) },
                    { 2, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250), null, "Starting", true, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250) },
                    { 3, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250), null, "Loading", true, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250) },
                    { 4, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(250), null, "Moving", true, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260) },
                    { 5, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260), null, "Unloading", true, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260) },
                    { 6, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260), null, "Completing", true, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260) },
                    { 7, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260), null, "Completed", true, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260) },
                    { 8, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260), null, "Aborted", true, new DateTime(2024, 3, 26, 21, 17, 59, 476, DateTimeKind.Utc).AddTicks(260) }
                });

            migrationBuilder.InsertData(
                table: "SystemComponents",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "api" },
                    { 2, "robot" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "SystemComponents",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "SystemComponents",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "System",
                table: "Progresses");
        }
    }
}
