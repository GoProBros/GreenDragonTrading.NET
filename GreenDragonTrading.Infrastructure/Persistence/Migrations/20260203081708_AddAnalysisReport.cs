using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalysisReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "logo_path",
                table: "symbols",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "analysis_report_categories",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    parent_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    display_order = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_report_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_analysis_report_categories_analysis_report_categories_paren~",
                        column: x => x.parent_id,
                        principalTable: "analysis_report_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "analysis_report_sources",
                columns: table => new
                {
                    id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    website = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    logo_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_report_sources", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "analysis_reports",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    category_id = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    title = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    tickers = table.Column<string[]>(type: "varchar(20)[]", nullable: true),
                    sector_id = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    publish_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    file_path = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    original_file_name = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    file_extension = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    mime_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_analysis_reports", x => x.id);
                    table.ForeignKey(
                        name: "FK_analysis_reports_analysis_report_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "analysis_report_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_analysis_reports_analysis_report_sources_source_id",
                        column: x => x.source_id,
                        principalTable: "analysis_report_sources",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_analysis_reports_sectors_sector_id",
                        column: x => x.sector_id,
                        principalTable: "sectors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_analysis_reports_users_uploaded_by",
                        column: x => x.uploaded_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "analysis_report_categories",
                columns: new[] { "id", "created_at", "description", "display_order", "name", "parent_id", "status", "updated_at" },
                values: new object[,]
                {
                    { "1000", new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5750), new TimeSpan(0, 0, 0, 0, 0)), "Phân tích kinh tế vĩ mô, chính sách, triển vọng thị trường", 0, "Báo cáo Vĩ mô", null, (short)1, null },
                    { "2000", new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5753), new TimeSpan(0, 0, 0, 0, 0)), "Phân tích các ngành công nghiệp, xu hướng và triển vọng", 0, "Báo cáo Ngành", null, (short)1, null },
                    { "3000", new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5756), new TimeSpan(0, 0, 0, 0, 0)), "Phân tích chuyên sâu về doanh nghiệp niêm yết", 0, "Báo cáo Doanh nghiệp", null, (short)1, null },
                    { "4000", new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5758), new TimeSpan(0, 0, 0, 0, 0)), "Chiến lược và khuyến nghị đầu tư", 0, "Chiến lược Đầu tư", null, (short)1, null },
                    { "5000", new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5760), new TimeSpan(0, 0, 0, 0, 0)), "Nhận định và phân tích thị trường chứng khoán", 0, "Báo cáo Thị trường", null, (short)1, null }
                });

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5578), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5579), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$twWnh/ija0AG2maKsZnVk.r6BiBQtIIic/HRJ.Cks85T4kkruR5c6");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5699), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5699), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_analysis_report_categories_display_order",
                table: "analysis_report_categories",
                column: "display_order");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_report_categories_parent_id",
                table: "analysis_report_categories",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_report_categories_status",
                table: "analysis_report_categories",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_report_sources_status",
                table: "analysis_report_sources",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_reports_category_id",
                table: "analysis_reports",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_reports_sector_id",
                table: "analysis_reports",
                column: "sector_id");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_reports_source_id",
                table: "analysis_reports",
                column: "source_id");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_reports_status",
                table: "analysis_reports",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_analysis_reports_uploaded_by",
                table: "analysis_reports",
                column: "uploaded_by");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "analysis_reports");

            migrationBuilder.DropTable(
                name: "analysis_report_categories");

            migrationBuilder.DropTable(
                name: "analysis_report_sources");

            migrationBuilder.DropColumn(
                name: "logo_path",
                table: "symbols");

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 31, 6, 3, 25, 293, DateTimeKind.Unspecified).AddTicks(3498), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 31, 6, 3, 25, 293, DateTimeKind.Unspecified).AddTicks(3499), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$Usr2LG6A.XQNGHN8fcGz8O9p5ncAA23FPVbk/DxV.NGF2runzos5y");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 31, 6, 3, 25, 293, DateTimeKind.Unspecified).AddTicks(3569), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 31, 6, 3, 25, 293, DateTimeKind.Unspecified).AddTicks(3570), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
