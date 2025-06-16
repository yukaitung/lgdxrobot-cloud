using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGDXRobotCloud.Data.Migrations
{
    /// <inheritdoc />
    public partial class HasWaypointsTrafficControl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasWaypointsTrafficControl",
                table: "Navigation.Realms",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "Navigation.Realms",
                keyColumn: "Id",
                keyValue: 1,
                column: "HasWaypointsTrafficControl",
                value: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasWaypointsTrafficControl",
                table: "Navigation.Realms");
        }
    }
}
