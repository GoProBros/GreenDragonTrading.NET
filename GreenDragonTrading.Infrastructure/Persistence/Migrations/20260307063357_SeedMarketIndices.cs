using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SeedMarketIndices : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 7, 6, 33, 56, 612, DateTimeKind.Unspecified).AddTicks(6099), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 7, 6, 33, 56, 612, DateTimeKind.Unspecified).AddTicks(6104), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 7, 6, 33, 56, 612, DateTimeKind.Unspecified).AddTicks(6107), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 7, 6, 33, 56, 612, DateTimeKind.Unspecified).AddTicks(6110), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 7, 6, 33, 56, 612, DateTimeKind.Unspecified).AddTicks(6112), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.InsertData(
                table: "market_indices",
                columns: new[] { "code", "created_at", "description", "exchange_code", "is_benchmark", "name", "status", "updated_at" },
                values: new object[,]
                {
                    { "HNX30", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Rổ 30 cổ phiếu có vốn hóa lớn nhất và thanh khoản cao nhất trên HNX.", "HNX", true, "Chỉ số HNX30", (short)1, null },
                    { "VN100", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Rổ 100 cổ phiếu có vốn hóa lớn nhất và thanh khoản cao nhất trên HOSE.", "HSX", false, "Chỉ số VN100", (short)1, null },
                    { "VN30", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Rổ 30 cổ phiếu có vốn hóa lớn nhất và thanh khoản cao nhất trên HOSE.", "HSX", true, "Chỉ số VN30", (short)1, null },
                    { "VNALL", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số bao gồm toàn bộ cổ phiếu niêm yết trên HOSE.", "HSX", false, "Chỉ số VN All-Share", (short)1, null },
                    { "VNMID", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số vốn hóa vừa, bao gồm các cổ phiếu từ hạng 31 đến 100 trên HOSE.", "HSX", false, "Chỉ số VN Mid-Cap", (short)1, null },
                    { "VNSI", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số phát triển bền vững, bao gồm các doanh nghiệp đạt tiêu chí ESG trên HOSE.", "HSX", false, "Chỉ số VN Sustainability", (short)1, null },
                    { "VNSML", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số vốn hóa nhỏ, bao gồm các cổ phiếu từ hạng 101 trở xuống trên HOSE.", "HSX", false, "Chỉ số VN Small-Cap", (short)1, null }
                });

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 3, 7, 6, 33, 56, 612, DateTimeKind.Unspecified).AddTicks(5768), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 7, 6, 33, 56, 612, DateTimeKind.Unspecified).AddTicks(5770), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$0fLy/9w32.0ORm7kU2R6lepA5KQqMWYIHegbnnYQg5WCQiej9j/2.");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 3, 7, 6, 33, 56, 612, DateTimeKind.Unspecified).AddTicks(6069), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 7, 6, 33, 56, 612, DateTimeKind.Unspecified).AddTicks(6070), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "HNX30");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VN100");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VN30");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNALL");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNMID");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNSI");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNSML");

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 7, 6, 20, 3, 899, DateTimeKind.Unspecified).AddTicks(5824), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 7, 6, 20, 3, 899, DateTimeKind.Unspecified).AddTicks(5828), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 7, 6, 20, 3, 899, DateTimeKind.Unspecified).AddTicks(5831), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 7, 6, 20, 3, 899, DateTimeKind.Unspecified).AddTicks(5834), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 7, 6, 20, 3, 899, DateTimeKind.Unspecified).AddTicks(5837), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 3, 7, 6, 20, 3, 899, DateTimeKind.Unspecified).AddTicks(5660), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 7, 6, 20, 3, 899, DateTimeKind.Unspecified).AddTicks(5661), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$EEIa4bg0qvmMXxKa1LZ9dOdEMvXkNoLvPxBVC7iUlcW6TyU6VPjL.");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 3, 7, 6, 20, 3, 899, DateTimeKind.Unspecified).AddTicks(5794), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 7, 6, 20, 3, 899, DateTimeKind.Unspecified).AddTicks(5795), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
