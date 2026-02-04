using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAnalysisReportCateSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "company");

            migrationBuilder.DeleteData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "industry");

            migrationBuilder.DeleteData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "macro");

            migrationBuilder.DeleteData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "market");

            migrationBuilder.DeleteData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "strategy");

            migrationBuilder.InsertData(
                table: "analysis_report_categories",
                columns: new[] { "id", "created_at", "description", "level", "name", "parent_id", "status", "updated_at" },
                values: new object[,]
                {
                    { "1000", new DateTimeOffset(new DateTime(2026, 2, 3, 10, 30, 21, 67, DateTimeKind.Unspecified).AddTicks(2058), new TimeSpan(0, 0, 0, 0, 0)), "Phân tích kinh tế vĩ mô, chính sách, triển vọng thị trường", 1, "Báo cáo Vĩ mô", null, (short)1, null },
                    { "2000", new DateTimeOffset(new DateTime(2026, 2, 3, 10, 30, 21, 67, DateTimeKind.Unspecified).AddTicks(2062), new TimeSpan(0, 0, 0, 0, 0)), "Phân tích các ngành công nghiệp, xu hướng và triển vọng", 1, "Báo cáo Ngành", null, (short)1, null },
                    { "3000", new DateTimeOffset(new DateTime(2026, 2, 3, 10, 30, 21, 67, DateTimeKind.Unspecified).AddTicks(2065), new TimeSpan(0, 0, 0, 0, 0)), "Phân tích chuyên sâu về doanh nghiệp niêm yết", 1, "Báo cáo Doanh nghiệp", null, (short)1, null },
                    { "4000", new DateTimeOffset(new DateTime(2026, 2, 3, 10, 30, 21, 67, DateTimeKind.Unspecified).AddTicks(2067), new TimeSpan(0, 0, 0, 0, 0)), "Chiến lược và khuyến nghị đầu tư", 1, "Chiến lược Đầu tư", null, (short)1, null },
                    { "5000", new DateTimeOffset(new DateTime(2026, 2, 3, 10, 30, 21, 67, DateTimeKind.Unspecified).AddTicks(2070), new TimeSpan(0, 0, 0, 0, 0)), "Nhận định và phân tích thị trường chứng khoán", 1, "Báo cáo Thị trường", null, (short)1, null }
                });

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 3, 10, 30, 21, 67, DateTimeKind.Unspecified).AddTicks(1918), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 3, 10, 30, 21, 67, DateTimeKind.Unspecified).AddTicks(1919), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$3RQ3pQiQswxchf/PEySf/urlkT64mwAjWRTyu1CE0e1TgMQd0mabe");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 3, 10, 30, 21, 67, DateTimeKind.Unspecified).AddTicks(2006), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 3, 10, 30, 21, 67, DateTimeKind.Unspecified).AddTicks(2006), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000");

            migrationBuilder.DeleteData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000");

            migrationBuilder.DeleteData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000");

            migrationBuilder.DeleteData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000");

            migrationBuilder.DeleteData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000");

            migrationBuilder.InsertData(
                table: "analysis_report_categories",
                columns: new[] { "id", "created_at", "description", "level", "name", "parent_id", "status", "updated_at" },
                values: new object[,]
                {
                    { "company", new DateTimeOffset(new DateTime(2026, 2, 3, 10, 27, 1, 6, DateTimeKind.Unspecified).AddTicks(5295), new TimeSpan(0, 0, 0, 0, 0)), "Phân tích chuyên sâu về doanh nghiệp niêm yết", 1, "Báo cáo Doanh nghiệp", null, (short)1, null },
                    { "industry", new DateTimeOffset(new DateTime(2026, 2, 3, 10, 27, 1, 6, DateTimeKind.Unspecified).AddTicks(5292), new TimeSpan(0, 0, 0, 0, 0)), "Phân tích các ngành công nghiệp, xu hướng và triển vọng", 1, "Báo cáo Ngành", null, (short)1, null },
                    { "macro", new DateTimeOffset(new DateTime(2026, 2, 3, 10, 27, 1, 6, DateTimeKind.Unspecified).AddTicks(5275), new TimeSpan(0, 0, 0, 0, 0)), "Phân tích kinh tế vĩ mô, chính sách, triển vọng thị trường", 1, "Báo cáo Vĩ mô", null, (short)1, null },
                    { "market", new DateTimeOffset(new DateTime(2026, 2, 3, 10, 27, 1, 6, DateTimeKind.Unspecified).AddTicks(5299), new TimeSpan(0, 0, 0, 0, 0)), "Nhận định và phân tích thị trường chứng khoán", 1, "Báo cáo Thị trường", null, (short)1, null },
                    { "strategy", new DateTimeOffset(new DateTime(2026, 2, 3, 10, 27, 1, 6, DateTimeKind.Unspecified).AddTicks(5297), new TimeSpan(0, 0, 0, 0, 0)), "Chiến lược và khuyến nghị đầu tư", 1, "Chiến lược Đầu tư", null, (short)1, null }
                });

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 3, 10, 27, 1, 6, DateTimeKind.Unspecified).AddTicks(4986), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 3, 10, 27, 1, 6, DateTimeKind.Unspecified).AddTicks(4987), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$nxkfJkpV6S0oq5tEAnHBYOtlEjNN0WWvy2fbTOIK0iDORZqd2BblC");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 3, 10, 27, 1, 6, DateTimeKind.Unspecified).AddTicks(5230), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 3, 10, 27, 1, 6, DateTimeKind.Unspecified).AddTicks(5231), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
