using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGDXRobot2Cloud.API.Migrations
{
    /// <inheritdoc />
    public partial class timePrecision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Waypoints",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Waypoints",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Triggers",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Triggers",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Robots",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Robots",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Progresses",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Progresses",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "NodesCollections",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "NodesCollections",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Nodes",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Nodes",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Flows",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Flows",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ApiKeys",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ApiKeys",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ApiKeyLocations",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ApiKeyLocations",
                type: "datetime(3)",
                precision: 3,
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Waypoints",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Waypoints",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Triggers",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Triggers",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Robots",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Robots",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Progresses",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Progresses",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "NodesCollections",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "NodesCollections",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Nodes",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Nodes",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "Flows",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Flows",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ApiKeys",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ApiKeys",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "ApiKeyLocations",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "ApiKeyLocations",
                type: "datetime(6)",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime(3)",
                oldPrecision: 3);

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
        }
    }
}
