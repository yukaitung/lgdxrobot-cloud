using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LGDXRobotCloud.Data.Migrations
{
    /// <inheritdoc />
    public partial class SkipOnFailure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkipOnFailure",
                table: "Automation.Triggers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SkipOnFailure",
                table: "Automation.Triggers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
