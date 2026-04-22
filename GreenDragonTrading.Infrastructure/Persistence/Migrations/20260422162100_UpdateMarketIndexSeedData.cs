using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMarketIndexSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "HNXUPCOMINDEX");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNALL");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNCOND");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNCONS");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNENE");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNFIN");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNFINLEAD");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNFINSELECT");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNHEAL");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNIND");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNIT");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNMAT");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNMID");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNREAL");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNSML");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNUTI");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNX50");

            migrationBuilder.DeleteData(
                table: "market_indices",
                keyColumn: "code",
                keyValue: "VNXALL");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 13, 36, 28, 526, DateTimeKind.Unspecified).AddTicks(78), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 13, 36, 28, 526, DateTimeKind.Unspecified).AddTicks(84), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 13, 36, 28, 526, DateTimeKind.Unspecified).AddTicks(87), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 13, 36, 28, 526, DateTimeKind.Unspecified).AddTicks(90), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 22, 13, 36, 28, 526, DateTimeKind.Unspecified).AddTicks(220), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.InsertData(
                table: "market_indices",
                columns: new[] { "code", "created_at", "description", "exchange_code", "is_benchmark", "name", "status", "updated_at" },
                values: new object[,]
                {
                    { "HNXUPCOMINDEX", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số tổng hợp của toàn bộ cổ phiếu đăng ký giao dịch trên thị trường UPCOM.", "UPCOM", true, "Chỉ số HNX UPCOM-Index", (short)1, null },
                    { "VNALL", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số bao gồm toàn bộ cổ phiếu niêm yết trên HOSE.", "HSX", false, "Chỉ số VN All-Share", (short)1, null },
                    { "VNCOND", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số ngành Hàng tiêu dùng không thiết yếu trên HOSE.", "HSX", false, "Chỉ số VN Consumer Discretionary", (short)1, null },
                    { "VNCONS", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số ngành Hàng tiêu dùng thiết yếu trên HOSE.", "HSX", false, "Chỉ số VN Consumer Staples", (short)1, null },
                    { "VNENE", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số ngành Năng lượng trên HOSE.", "HSX", false, "Chỉ số VN Energy", (short)1, null },
                    { "VNFIN", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số ngành Tài chính trên HOSE.", "HSX", false, "Chỉ số VN Financials", (short)1, null },
                    { "VNFINLEAD", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Rổ cổ phiếu dẫn đầu ngành Tài chính trên HOSE.", "HSX", false, "Chỉ số VN Financial Leaders", (short)1, null },
                    { "VNFINSELECT", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Rổ cổ phiếu tuyển chọn ngành Tài chính trên HOSE.", "HSX", false, "Chỉ số VN Financial Select", (short)1, null },
                    { "VNHEAL", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số ngành Y tế và Dược phẩm trên HOSE.", "HSX", false, "Chỉ số VN Healthcare", (short)1, null },
                    { "VNIND", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số ngành Công nghiệp trên HOSE.", "HSX", false, "Chỉ số VN Industrials", (short)1, null },
                    { "VNIT", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số ngành Công nghệ thông tin trên HOSE.", "HSX", false, "Chỉ số VN Information Technology", (short)1, null },
                    { "VNMAT", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số ngành Nguyên vật liệu trên HOSE.", "HSX", false, "Chỉ số VN Materials", (short)1, null },
                    { "VNMID", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số vốn hóa vừa, bao gồm các cổ phiếu từ hạng 31 đến 100 trên HOSE.", "HSX", false, "Chỉ số VN Mid-Cap", (short)1, null },
                    { "VNREAL", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số ngành Bất động sản trên HOSE.", "HSX", false, "Chỉ số VN Real Estate", (short)1, null },
                    { "VNSML", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số vốn hóa nhỏ, bao gồm các cổ phiếu từ hạng 101 trở xuống trên HOSE.", "HSX", false, "Chỉ số VN Small-Cap", (short)1, null },
                    { "VNUTI", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số ngành Tiện ích công cộng trên HOSE.", "HSX", false, "Chỉ số VN Utilities", (short)1, null },
                    { "VNX50", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Rổ 50 cổ phiếu hàng đầu trên cả HOSE và HNX.", "HSX", false, "Chỉ số VNX50", (short)1, null },
                    { "VNXALL", new DateTime(2026, 3, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Chỉ số bao gồm toàn bộ cổ phiếu niêm yết trên cả HOSE và HNX.", "HSX", false, "Chỉ số VNX All-Share", (short)1, null }
                });

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 4, 22, 13, 36, 28, 525, DateTimeKind.Unspecified).AddTicks(9947), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 4, 22, 13, 36, 28, 525, DateTimeKind.Unspecified).AddTicks(9948), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$aQRL9Bcx/3l2IFsmMJjwPu.SUwSiyFB6FXOrjGDf/wbJx1Q7cP69i");
        }
    }
}
