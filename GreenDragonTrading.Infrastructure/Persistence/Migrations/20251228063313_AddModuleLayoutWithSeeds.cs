using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleLayoutWithSeeds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"));

            migrationBuilder.CreateTable(
                name: "module_layouts",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: true),
                    module_type = table.Column<short>(type: "smallint", maxLength: 50, nullable: false),
                    layout_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    config_json = table.Column<string>(type: "jsonb", nullable: false),
                    is_system_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_module_layouts", x => x.id);
                    table.ForeignKey(
                        name: "FK_module_layouts_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "module_layouts",
                columns: new[] { "id", "config_json", "created_at", "is_system_default", "layout_name", "module_type", "updated_at", "user_id" },
                values: new object[] { 1L, "\r\n        {\r\n          \"state\": {\r\n            \"columns\": {\r\n              \"ticker\": { \"field\": \"ticker\", \"visible\": true, \"width\": 80, \"order\": 0 },\r\n              \"lastPrice\": { \"field\": \"lastPrice\", \"visible\": true, \"width\": 95, \"order\": 10 },\r\n              \"change\": { \"field\": \"change\", \"visible\": true, \"width\": 80, \"order\": 12 },\r\n              \"ratioChange\": { \"field\": \"ratioChange\", \"visible\": true, \"width\": 90, \"order\": 13 },\r\n              \"totalVol\": { \"field\": \"totalVol\", \"visible\": true, \"width\": 120, \"order\": 20 },\r\n              \"PE\": { \"field\": \"PE\", \"visible\": false, \"width\": 80, \"order\": 48 },\r\n              \"ROE\": { \"field\": \"ROE\", \"visible\": false, \"width\": 80, \"order\": 49 }\r\n            }\r\n          }\r\n        }", new DateTimeOffset(new DateTime(2025, 12, 28, 6, 33, 13, 356, DateTimeKind.Unspecified).AddTicks(794), new TimeSpan(0, 0, 0, 0, 0)), true, "Giao diện bộ lọc mặc định", (short)1, new DateTimeOffset(new DateTime(2025, 12, 28, 6, 33, 13, 356, DateTimeKind.Unspecified).AddTicks(794), new TimeSpan(0, 0, 0, 0, 0)), null });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "avatar_url", "created_at", "email", "hashed_password", "is_email_verified", "phone_number", "role", "status", "username" },
                values: new object[] { new Guid("f0a0988e-cc8d-406f-9868-b014f5bc4689"), null, new DateTimeOffset(new DateTime(2025, 12, 28, 6, 33, 13, 356, DateTimeKind.Unspecified).AddTicks(754), new TimeSpan(0, 0, 0, 0, 0)), "greendragon.trading.team@gmail.com", "96cae35ce8a9b0244178bf28e4966c2ce1b8385723a96a6b838858cdd6ca0a1e", true, "0988671875", (short)3, (short)1, "admin" });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "layout_json", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 12, 28, 6, 33, 13, 356, DateTimeKind.Unspecified).AddTicks(831), new TimeSpan(0, 0, 0, 0, 0)), "{\r\n          \"modules\": [\r\n            {\r\n              \"i\": \"stock-screener-default\",\r\n              \"type\": \"stock-screener\",\r\n              \"title\": \"Bộ lọc cổ phiếu\",\r\n              \"x\": 0, \"y\": 36, \"w\": 96, \"h\": 20,\r\n              \"activeLayoutId\": 1  \r\n            }\r\n          ]\r\n        }", new DateTimeOffset(new DateTime(2025, 12, 28, 6, 33, 13, 356, DateTimeKind.Unspecified).AddTicks(832), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_user_id_is_default",
                table: "workspaces",
                columns: new[] { "user_id", "is_default" },
                filter: "\"is_default\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_module_layouts_is_system_default",
                table: "module_layouts",
                column: "is_system_default",
                filter: "\"is_system_default\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_module_layouts_user_id_module_type",
                table: "module_layouts",
                columns: new[] { "user_id", "module_type" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "module_layouts");

            migrationBuilder.DropIndex(
                name: "IX_workspaces_user_id_is_default",
                table: "workspaces");

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("f0a0988e-cc8d-406f-9868-b014f5bc4689"));

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "avatar_url", "created_at", "email", "hashed_password", "is_email_verified", "phone_number", "role", "status", "username" },
                values: new object[] { new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"), null, new DateTimeOffset(new DateTime(2025, 12, 16, 6, 33, 37, 606, DateTimeKind.Unspecified).AddTicks(7844), new TimeSpan(0, 0, 0, 0, 0)), "greendragon.trading.team@gmail.com", "96cae35ce8a9b0244178bf28e4966c2ce1b8385723a96a6b838858cdd6ca0a1e", true, "0988671875", (short)3, (short)1, "admin" });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "layout_json", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 12, 16, 6, 33, 37, 606, DateTimeKind.Unspecified).AddTicks(7870), new TimeSpan(0, 0, 0, 0, 0)), "{\r\n          \"modules\": [\r\n            { \"id\": \"watchList\", \"position\": { \"x\": 0, \"y\": 0, \"w\": 6, \"h\": 4 } },\r\n            { \"id\": \"chart\", \"position\": { \"x\": 6, \"y\": 0, \"w\": 6, \"h\": 8 } },\r\n            { \"id\": \"orderBook\", \"position\": { \"x\": 0, \"y\": 4, \"w\": 6, \"h\": 4 } }\r\n          ]\r\n        }", new DateTimeOffset(new DateTime(2025, 12, 16, 6, 33, 37, 606, DateTimeKind.Unspecified).AddTicks(7870), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
