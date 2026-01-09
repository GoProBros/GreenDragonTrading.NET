using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSubscriptionAndWorkspaceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a050ea7b-2c96-4108-9352-cdb997ffb8d5"));

            migrationBuilder.CreateTable(
                name: "subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    level_order = table.Column<int>(type: "integer", nullable: false),
                    max_layouts = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    duration_in_days = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subscriptions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "workspaces",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    workspace_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    layout_json = table.Column<string>(type: "jsonb", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    share_code = table.Column<string>(type: "varchar(8)", maxLength: 8, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workspaces", x => x.id);
                    table.ForeignKey(
                        name: "FK_workspaces_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_subscriptions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_subscriptions", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_subscriptions_subscriptions_subscription_id",
                        column: x => x.subscription_id,
                        principalTable: "subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_subscriptions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "avatar_url", "created_at", "email", "hashed_password", "is_email_verified", "phone_number", "role", "status", "username" },
                values: new object[] { new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"), null, new DateTimeOffset(new DateTime(2025, 12, 16, 6, 33, 37, 606, DateTimeKind.Unspecified).AddTicks(7844), new TimeSpan(0, 0, 0, 0, 0)), "greendragon.trading.team@gmail.com", "96cae35ce8a9b0244178bf28e4966c2ce1b8385723a96a6b838858cdd6ca0a1e", true, "0988671875", (short)3, (short)1, "admin" });

            migrationBuilder.InsertData(
                table: "workspaces",
                columns: new[] { "id", "created_at", "is_default", "layout_json", "share_code", "updated_at", "user_id", "workspace_name" },
                values: new object[] { 1, new DateTimeOffset(new DateTime(2025, 12, 16, 6, 33, 37, 606, DateTimeKind.Unspecified).AddTicks(7870), new TimeSpan(0, 0, 0, 0, 0)), true, "{\r\n          \"modules\": [\r\n            { \"id\": \"watchList\", \"position\": { \"x\": 0, \"y\": 0, \"w\": 6, \"h\": 4 } },\r\n            { \"id\": \"chart\", \"position\": { \"x\": 6, \"y\": 0, \"w\": 6, \"h\": 8 } },\r\n            { \"id\": \"orderBook\", \"position\": { \"x\": 0, \"y\": 4, \"w\": 6, \"h\": 4 } }\r\n          ]\r\n        }", null, new DateTimeOffset(new DateTime(2025, 12, 16, 6, 33, 37, 606, DateTimeKind.Unspecified).AddTicks(7870), new TimeSpan(0, 0, 0, 0, 0)), null, "SYSTEM_DEFAULT_LAYOUT" });

            migrationBuilder.CreateIndex(
                name: "IX_user_subscriptions_subscription_id",
                table: "user_subscriptions",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_subscriptions_user_id",
                table: "user_subscriptions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_share_code",
                table: "workspaces",
                column: "share_code",
                unique: true,
                filter: "\"share_code\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_user_id_workspace_name",
                table: "workspaces",
                columns: new[] { "user_id", "workspace_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "user_subscriptions");

            migrationBuilder.DropTable(
                name: "workspaces");

            migrationBuilder.DropTable(
                name: "subscriptions");

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"));

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "avatar_url", "created_at", "email", "hashed_password", "is_email_verified", "phone_number", "role", "status", "username" },
                values: new object[] { new Guid("a050ea7b-2c96-4108-9352-cdb997ffb8d5"), null, new DateTimeOffset(new DateTime(2025, 12, 14, 6, 54, 21, 839, DateTimeKind.Unspecified).AddTicks(6803), new TimeSpan(0, 0, 0, 0, 0)), "greendragon.trading.team@gmail.com", "96cae35ce8a9b0244178bf28e4966c2ce1b8385723a96a6b838858cdd6ca0a1e", true, "0988671875", (short)3, (short)1, "admin" });
        }
    }
}
