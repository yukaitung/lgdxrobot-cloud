using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LGDXRobot2Cloud.API.Migrations
{
    /// <inheritdoc />
    public partial class apiLocation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "ApiKeyLocations");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ApiKeyLocations");

            migrationBuilder.InsertData(
                table: "ApiKeyLocations",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "header" },
                    { 2, "body" }
                });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7120), new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7120) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7120), new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7120) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7120), new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7120) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7120), new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7120) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7130), new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7130) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7130), new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7130) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7130), new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7130) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7130), new DateTime(2024, 3, 29, 22, 13, 38, 542, DateTimeKind.Utc).AddTicks(7130) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ApiKeyLocations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ApiKeyLocations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "ApiKeyLocations",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ApiKeyLocations",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1320), new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1320) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1330), new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1330) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1330), new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1330) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1330), new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1330) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1330), new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1330) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1330), new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1330) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1330), new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1330) });

            migrationBuilder.UpdateData(
                table: "Progresses",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "CreatedAt", "UpdatedAt" },
                values: new object[] { new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1340), new DateTime(2024, 3, 29, 14, 43, 41, 119, DateTimeKind.Utc).AddTicks(1340) });
        }
    }
}
