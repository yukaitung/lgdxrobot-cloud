using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LGDXRobot2Cloud.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Administration.ApiKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Secret = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsThirdParty = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Administration.ApiKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Automation.Flows",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automation.Flows", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Automation.Progresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    System = table.Column<bool>(type: "boolean", nullable: false),
                    Reserved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automation.Progresses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Navigation.Realms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Image = table.Column<byte[]>(type: "bytea", nullable: false),
                    Resolution = table.Column<double>(type: "double precision", nullable: false),
                    OriginX = table.Column<double>(type: "double precision", nullable: false),
                    OriginY = table.Column<double>(type: "double precision", nullable: false),
                    OriginRotation = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navigation.Realms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Automation.Triggers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Url = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    HttpMethodId = table.Column<int>(type: "integer", nullable: false),
                    Body = table.Column<string>(type: "text", nullable: true),
                    SkipOnFailure = table.Column<bool>(type: "boolean", nullable: false),
                    ApiKeyInsertLocationId = table.Column<int>(type: "integer", nullable: true),
                    ApiKeyFieldName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ApiKeyId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automation.Triggers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Automation.Triggers_Administration.ApiKeys_ApiKeyId",
                        column: x => x.ApiKeyId,
                        principalTable: "Administration.ApiKeys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Navigation.Robots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RealmId = table.Column<int>(type: "integer", nullable: false),
                    IsRealtimeExchange = table.Column<bool>(type: "boolean", nullable: false),
                    IsProtectingHardwareSerialNumber = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navigation.Robots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Navigation.Robots_Navigation.Realms_RealmId",
                        column: x => x.RealmId,
                        principalTable: "Navigation.Realms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Navigation.Waypoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RealmId = table.Column<int>(type: "integer", nullable: false),
                    X = table.Column<double>(type: "double precision", nullable: false),
                    Y = table.Column<double>(type: "double precision", nullable: false),
                    Rotation = table.Column<double>(type: "double precision", nullable: false),
                    IsParking = table.Column<bool>(type: "boolean", nullable: false),
                    HasCharger = table.Column<bool>(type: "boolean", nullable: false),
                    IsReserved = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navigation.Waypoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Navigation.Waypoints_Navigation.Realms_RealmId",
                        column: x => x.RealmId,
                        principalTable: "Navigation.Realms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Automation.FlowDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    ProgressId = table.Column<int>(type: "integer", nullable: false),
                    AutoTaskNextControllerId = table.Column<int>(type: "integer", nullable: false),
                    TriggerId = table.Column<int>(type: "integer", nullable: true),
                    FlowId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automation.FlowDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Automation.FlowDetails_Automation.Flows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Automation.Flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Automation.FlowDetails_Automation.Progresses_ProgressId",
                        column: x => x.ProgressId,
                        principalTable: "Automation.Progresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Automation.FlowDetails_Automation.Triggers_TriggerId",
                        column: x => x.TriggerId,
                        principalTable: "Automation.Triggers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Automation.AutoTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    FlowId = table.Column<int>(type: "integer", nullable: true),
                    RealmId = table.Column<int>(type: "integer", nullable: false),
                    AssignedRobotId = table.Column<Guid>(type: "uuid", nullable: true),
                    CurrentProgressId = table.Column<int>(type: "integer", nullable: false),
                    CurrentProgressOrder = table.Column<int>(type: "integer", nullable: true),
                    NextToken = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automation.AutoTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Automation.AutoTasks_Automation.Flows_FlowId",
                        column: x => x.FlowId,
                        principalTable: "Automation.Flows",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Automation.AutoTasks_Automation.Progresses_CurrentProgressId",
                        column: x => x.CurrentProgressId,
                        principalTable: "Automation.Progresses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Automation.AutoTasks_Navigation.Realms_RealmId",
                        column: x => x.RealmId,
                        principalTable: "Navigation.Realms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Automation.AutoTasks_Navigation.Robots_AssignedRobotId",
                        column: x => x.AssignedRobotId,
                        principalTable: "Navigation.Robots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Navigation.RobotCertificates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Thumbprint = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ThumbprintBackup = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    NotBefore = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    NotAfter = table.Column<DateTime>(type: "timestamp(0) with time zone", precision: 0, nullable: false),
                    RobotId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navigation.RobotCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Navigation.RobotCertificates_Navigation.Robots_RobotId",
                        column: x => x.RobotId,
                        principalTable: "Navigation.Robots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Navigation.RobotChassisInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RobotTypeId = table.Column<int>(type: "integer", nullable: false),
                    ChassisLengthX = table.Column<double>(type: "double precision", nullable: false),
                    ChassisLengthY = table.Column<double>(type: "double precision", nullable: false),
                    ChassisWheelCount = table.Column<int>(type: "integer", nullable: false),
                    ChassisWheelRadius = table.Column<double>(type: "double precision", nullable: false),
                    BatteryCount = table.Column<int>(type: "integer", nullable: false),
                    BatteryMaxVoltage = table.Column<double>(type: "double precision", nullable: false),
                    BatteryMinVoltage = table.Column<double>(type: "double precision", nullable: false),
                    RobotId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navigation.RobotChassisInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Navigation.RobotChassisInfos_Navigation.Robots_RobotId",
                        column: x => x.RobotId,
                        principalTable: "Navigation.Robots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Navigation.RobotSystemInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Cpu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsLittleEndian = table.Column<bool>(type: "boolean", nullable: false),
                    Motherboard = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MotherboardSerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RamMiB = table.Column<int>(type: "integer", nullable: false),
                    Gpu = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Os = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Is32Bit = table.Column<bool>(type: "boolean", nullable: false),
                    McuSerialNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RobotId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Navigation.RobotSystemInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Navigation.RobotSystemInfos_Navigation.Robots_RobotId",
                        column: x => x.RobotId,
                        principalTable: "Navigation.Robots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Automation.AutoTaskDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CustomX = table.Column<double>(type: "double precision", nullable: true),
                    CustomY = table.Column<double>(type: "double precision", nullable: true),
                    CustomRotation = table.Column<double>(type: "double precision", nullable: true),
                    WaypointId = table.Column<int>(type: "integer", nullable: true),
                    AutoTaskId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automation.AutoTaskDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Automation.AutoTaskDetails_Automation.AutoTasks_AutoTaskId",
                        column: x => x.AutoTaskId,
                        principalTable: "Automation.AutoTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Automation.AutoTaskDetails_Navigation.Waypoints_WaypointId",
                        column: x => x.WaypointId,
                        principalTable: "Navigation.Waypoints",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Automation.TriggerRetries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TriggerId = table.Column<int>(type: "integer", nullable: false),
                    AutoTaskId = table.Column<int>(type: "integer", nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp(3) with time zone", precision: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Automation.TriggerRetries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Automation.TriggerRetries_Automation.AutoTasks_AutoTaskId",
                        column: x => x.AutoTaskId,
                        principalTable: "Automation.AutoTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Automation.TriggerRetries_Automation.Triggers_TriggerId",
                        column: x => x.TriggerId,
                        principalTable: "Automation.Triggers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Automation.Progresses",
                columns: new[] { "Id", "Name", "Reserved", "System" },
                values: new object[,]
                {
                    { 1, "Template", true, true },
                    { 2, "Waiting", true, true },
                    { 3, "Completed", true, true },
                    { 4, "Aborted", true, true },
                    { 5, "Starting", false, true },
                    { 6, "Loading", false, true },
                    { 7, "PreMoving", false, true },
                    { 8, "Moving", false, true },
                    { 9, "Unloading", false, true },
                    { 10, "Completing", false, true },
                    { 99, "Reserved", true, true }
                });

            migrationBuilder.InsertData(
                table: "Navigation.Realms",
                columns: new[] { "Id", "Description", "Image", "Name", "OriginRotation", "OriginX", "OriginY", "Resolution" },
                values: new object[] { 1, "Please update this realm", new byte[0], "First Realm", 0.0, 0.0, 0.0, 0.0 });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Automation.AutoTaskDetails_AutoTaskId",
                table: "Automation.AutoTaskDetails",
                column: "AutoTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.AutoTaskDetails_Order",
                table: "Automation.AutoTaskDetails",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.AutoTaskDetails_WaypointId",
                table: "Automation.AutoTaskDetails",
                column: "WaypointId");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.AutoTasks_AssignedRobotId",
                table: "Automation.AutoTasks",
                column: "AssignedRobotId");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.AutoTasks_CurrentProgressId",
                table: "Automation.AutoTasks",
                column: "CurrentProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.AutoTasks_FlowId",
                table: "Automation.AutoTasks",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.AutoTasks_RealmId_AssignedRobotId",
                table: "Automation.AutoTasks",
                columns: new[] { "RealmId", "AssignedRobotId" });

            migrationBuilder.CreateIndex(
                name: "IX_Automation.FlowDetails_FlowId",
                table: "Automation.FlowDetails",
                column: "FlowId");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.FlowDetails_Order",
                table: "Automation.FlowDetails",
                column: "Order");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.FlowDetails_ProgressId",
                table: "Automation.FlowDetails",
                column: "ProgressId");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.FlowDetails_TriggerId",
                table: "Automation.FlowDetails",
                column: "TriggerId");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.TriggerRetries_AutoTaskId",
                table: "Automation.TriggerRetries",
                column: "AutoTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.TriggerRetries_TriggerId",
                table: "Automation.TriggerRetries",
                column: "TriggerId");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.Triggers_ApiKeyId",
                table: "Automation.Triggers",
                column: "ApiKeyId");

            migrationBuilder.CreateIndex(
                name: "IX_Automation.Triggers_ApiKeyInsertLocationId",
                table: "Automation.Triggers",
                column: "ApiKeyInsertLocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Navigation.RobotCertificates_RobotId",
                table: "Navigation.RobotCertificates",
                column: "RobotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Navigation.RobotChassisInfos_RobotId",
                table: "Navigation.RobotChassisInfos",
                column: "RobotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Navigation.Robots_RealmId",
                table: "Navigation.Robots",
                column: "RealmId");

            migrationBuilder.CreateIndex(
                name: "IX_Navigation.RobotSystemInfos_RobotId",
                table: "Navigation.RobotSystemInfos",
                column: "RobotId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Navigation.Waypoints_RealmId",
                table: "Navigation.Waypoints",
                column: "RealmId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Automation.AutoTaskDetails");

            migrationBuilder.DropTable(
                name: "Automation.FlowDetails");

            migrationBuilder.DropTable(
                name: "Automation.TriggerRetries");

            migrationBuilder.DropTable(
                name: "Navigation.RobotCertificates");

            migrationBuilder.DropTable(
                name: "Navigation.RobotChassisInfos");

            migrationBuilder.DropTable(
                name: "Navigation.RobotSystemInfos");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Navigation.Waypoints");

            migrationBuilder.DropTable(
                name: "Automation.AutoTasks");

            migrationBuilder.DropTable(
                name: "Automation.Triggers");

            migrationBuilder.DropTable(
                name: "Automation.Flows");

            migrationBuilder.DropTable(
                name: "Automation.Progresses");

            migrationBuilder.DropTable(
                name: "Navigation.Robots");

            migrationBuilder.DropTable(
                name: "Administration.ApiKeys");

            migrationBuilder.DropTable(
                name: "Navigation.Realms");
        }
    }
}
