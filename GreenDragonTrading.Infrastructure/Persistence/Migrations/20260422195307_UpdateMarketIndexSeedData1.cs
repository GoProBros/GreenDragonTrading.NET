using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMarketIndexSeedData1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "HNXINDEX");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNINDEX");

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 19, 53, 6, 618, DateTimeKind.Unspecified).AddTicks(481), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 19, 53, 6, 618, DateTimeKind.Unspecified).AddTicks(513), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 19, 53, 6, 618, DateTimeKind.Unspecified).AddTicks(516), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 19, 53, 6, 618, DateTimeKind.Unspecified).AddTicks(1061), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 19, 53, 6, 618, DateTimeKind.Unspecified).AddTicks(1066), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.InsertData(
                table: "market_indices",
                columns: new[] { "code", "created_at", "description", "exchange_code", "is_benchmark", "name", "status", "updated_at" },
                values: new object[,]
                {
                    { "HNXIndex", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số tổng hợp của toàn bộ cổ phiếu niêm yết trên Sở Giao dịch Chứng khoán Hà Nội (HNX).", "HNX", true, "Chỉ số HNX-Index", (short)1, null },
                    { "VNIndex", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số tổng hợp của toàn bộ cổ phiếu niêm yết trên Sở Giao dịch Chứng khoán TP.HCM (HOSE).", "HSX", true, "Chỉ số VN-Index", (short)1, null }
                });

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 4, 22, 19, 53, 6, 618, DateTimeKind.Unspecified).AddTicks(337), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 4, 22, 19, 53, 6, 618, DateTimeKind.Unspecified).AddTicks(338), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$1/wKhy5SHBloL4EorabPieN65xwwKNwlGDzJwxc7Yl4Q2djc.PopK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "HNXIndex");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNIndex");

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 16, 20, 57, 323, DateTimeKind.Unspecified).AddTicks(4869), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 16, 20, 57, 323, DateTimeKind.Unspecified).AddTicks(4884), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 16, 20, 57, 323, DateTimeKind.Unspecified).AddTicks(4887), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 16, 20, 57, 323, DateTimeKind.Unspecified).AddTicks(4890), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 16, 20, 57, 323, DateTimeKind.Unspecified).AddTicks(4893), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.InsertData(
                table: "market_indices",
                columns: new[] { "code", "created_at", "description", "exchange_code", "is_benchmark", "name", "status", "updated_at" },
                values: new object[,]
                {
                    { "HNXINDEX", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số tổng hợp của toàn bộ cổ phiếu niêm yết trên Sở Giao dịch Chứng khoán Hà Nội (HNX).", "HNX", true, "Chỉ số HNX-Index", (short)1, null },
                    { "VNINDEX", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số tổng hợp của toàn bộ cổ phiếu niêm yết trên Sở Giao dịch Chứng khoán TP.HCM (HOSE).", "HSX", true, "Chỉ số VN-Index", (short)1, null }
                });

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 4, 22, 16, 20, 57, 323, DateTimeKind.Unspecified).AddTicks(4611), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 4, 22, 16, 20, 57, 323, DateTimeKind.Unspecified).AddTicks(4612), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$zTXwSGL9ulc710OvfYPF7Oh31rHtVYx2rtbK.CwUZt8omyYSwDfYq");
        }
    }
}
