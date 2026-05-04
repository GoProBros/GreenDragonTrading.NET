using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkspaceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workspaces_user_id_is_default",
                table: "workspaces");

            migrationBuilder.AddColumn<short>(
                name: "type",
                table: "workspaces",
                type: "smallint",
                nullable: false,
                defaultValue: (short)1);

            
            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "layout_json", "type", "updated_at", "workspace_name" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "{\r\n          \"modules\": [\r\n            {\r\n              \"i\": \"stock-screener-web\",\r\n              \"type\": \"stock-screener\",\r\n              \"title\": \"Bộ lọc cổ phiếu (Web)\",\r\n              \"x\": 0, \"y\": 0, \"w\": 96, \"h\": 20\r\n            }\r\n          ]\r\n        }", (short)1, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "DEFAULT_WEB_LAYOUT" });

            migrationBuilder.InsertData(
                table: "workspaces",
                columns: new[] { "id", "created_at", "is_default", "layout_json", "share_code", "type", "updated_at", "user_id", "workspace_name" },
                values: new object[] { 2, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, "{\r\n          \"modules\": [\r\n            {\r\n              \"i\": \"stock-list-mobile\",\r\n              \"type\": \"stock-list\",\r\n              \"title\": \"Danh mục theo dõi (Mobile)\",\r\n              \"x\": 0, \"y\": 0, \"w\": 12, \"h\": 10\r\n            }\r\n          ]\r\n        }", null, (short)2, new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "DEFAULT_MOBILE_LAYOUT" });

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_user_id_type_is_default",
                table: "workspaces",
                columns: new[] { "user_id", "type", "is_default" },
                unique: true,
                filter: "\"is_default\" = true");

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_user_id_type_workspace_name",
                table: "workspaces",
                columns: new[] { "user_id", "type", "workspace_name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_workspaces_user_id_type_is_default",
                table: "workspaces");

            migrationBuilder.DropIndex(
                name: "IX_workspaces_user_id_type_workspace_name",
                table: "workspaces");

            migrationBuilder.DeleteData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DropColumn(
                name: "type",
                table: "workspaces");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "layout_json", "updated_at", "workspace_name" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 3, 28, 2, 39, 34, 262, DateTimeKind.Unspecified).AddTicks(6238), new TimeSpan(0, 0, 0, 0, 0)), "{\r\n          \"modules\": [\r\n            {\r\n              \"i\": \"stock-screener-default\",\r\n              \"type\": \"stock-screener\",\r\n              \"title\": \"Bộ lọc cổ phiếu\",\r\n              \"x\": 0, \"y\": 36, \"w\": 96, \"h\": 20,\r\n              \"activeLayoutId\": 1  \r\n            }\r\n          ]\r\n        }", new DateTimeOffset(new DateTime(2026, 3, 28, 2, 39, 34, 262, DateTimeKind.Unspecified).AddTicks(6239), new TimeSpan(0, 0, 0, 0, 0)), "SYSTEM_DEFAULT_LAYOUT" });

            migrationBuilder.CreateIndex(
                name: "IX_workspaces_user_id_is_default",
                table: "workspaces",
                columns: new[] { "user_id", "is_default" },
                filter: "\"is_default\" = true");
        }
    }
}
