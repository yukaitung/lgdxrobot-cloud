﻿using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace LGDXRobotCloud.Data.Migrations
{
    /// <inheritdoc />
    public partial class WaypointTraffic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Navigation.WaypointLinks");

            migrationBuilder.CreateTable(
                name: "Navigation.WaypointTraffics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RealmId = table.Column<int>(type: "integer", nullable: false),
                    WaypointFromId = table.Column<int>(type: "integer", nullable: false),
                    WaypointToId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navigation.WaypointTraffics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Navigation.WaypointTraffics_Navigation.Realms_RealmId",
                        column: x => x.RealmId,
                        principalTable: "Navigation.Realms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Navigation.WaypointTraffics_Navigation.Waypoints_WaypointFr~",
                        column: x => x.WaypointFromId,
                        principalTable: "Navigation.Waypoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Navigation.WaypointTraffics_Navigation.Waypoints_WaypointTo~",
                        column: x => x.WaypointToId,
                        principalTable: "Navigation.Waypoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Navigation.WaypointTraffics_RealmId",
                table: "Navigation.WaypointTraffics",
                column: "RealmId");

            migrationBuilder.CreateIndex(
                name: "IX_Navigation.WaypointTraffics_WaypointFromId",
                table: "Navigation.WaypointTraffics",
                column: "WaypointFromId");

            migrationBuilder.CreateIndex(
                name: "IX_Navigation.WaypointTraffics_WaypointToId",
                table: "Navigation.WaypointTraffics",
                column: "WaypointToId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Navigation.WaypointTraffics");

            migrationBuilder.CreateTable(
                name: "Navigation.WaypointLinks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RealmId = table.Column<int>(type: "integer", nullable: false),
                    WaypointFromId = table.Column<int>(type: "integer", nullable: false),
                    WaypointToId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navigation.WaypointLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Navigation.WaypointLinks_Navigation.Realms_RealmId",
                        column: x => x.RealmId,
                        principalTable: "Navigation.Realms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Navigation.WaypointLinks_Navigation.Waypoints_WaypointFromId",
                        column: x => x.WaypointFromId,
                        principalTable: "Navigation.Waypoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Navigation.WaypointLinks_Navigation.Waypoints_WaypointToId",
                        column: x => x.WaypointToId,
                        principalTable: "Navigation.Waypoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Navigation.WaypointLinks_RealmId",
                table: "Navigation.WaypointLinks",
                column: "RealmId");

            migrationBuilder.CreateIndex(
                name: "IX_Navigation.WaypointLinks_WaypointFromId",
                table: "Navigation.WaypointLinks",
                column: "WaypointFromId");

            migrationBuilder.CreateIndex(
                name: "IX_Navigation.WaypointLinks_WaypointToId",
                table: "Navigation.WaypointLinks",
                column: "WaypointToId");
        }
    }
}
