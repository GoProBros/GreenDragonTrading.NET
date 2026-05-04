using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketIndexTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "market_indices",
                columns: table => new
                {
                    code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    exchange_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    is_benchmark = table.Column<bool>(type: "boolean", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_market_indices", x => x.code);
                    table.ForeignKey(
                        name: "FK_market_indices_exchanges_exchange_code",
                        column: x => x.exchange_code,
                        principalTable: "exchanges",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "market_index_symbols",
                columns: table => new
                {
                    index_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    ticker = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    weight = table.Column<decimal>(type: "numeric(10,6)", nullable: true),
                    added_date = table.Column<DateOnly>(type: "date", nullable: true),
                    removed_date = table.Column<DateOnly>(type: "date", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_market_index_symbols", x => new { x.index_code, x.ticker });
                    table.ForeignKey(
                        name: "FK_market_index_symbols_market_indices_index_code",
                        column: x => x.index_code,
                        principalTable: "market_indices",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_market_index_symbols_symbols_ticker",
                        column: x => x.ticker,
                        principalTable: "symbols",
                        principalColumn: "ticker",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_market_index_symbols_index_code",
                table: "market_index_symbols",
                column: "index_code");

            migrationBuilder.CreateIndex(
                name: "IX_market_index_symbols_index_code_is_active",
                table: "market_index_symbols",
                columns: new[] { "index_code", "is_active" });

            migrationBuilder.CreateIndex(
                name: "IX_market_index_symbols_is_active",
                table: "market_index_symbols",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "IX_market_index_symbols_ticker",
                table: "market_index_symbols",
                column: "ticker");

            migrationBuilder.CreateIndex(
                name: "IX_market_indices_exchange_code",
                table: "market_indices",
                column: "exchange_code");

            migrationBuilder.CreateIndex(
                name: "IX_market_indices_is_benchmark",
                table: "market_indices",
                column: "is_benchmark");

            migrationBuilder.CreateIndex(
                name: "IX_market_indices_status",
                table: "market_indices",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "market_index_symbols");

            migrationBuilder.DropTable(
                name: "market_indices");

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 2, 12, 22, 50, 119, DateTimeKind.Unspecified).AddTicks(3834), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 2, 12, 22, 50, 119, DateTimeKind.Unspecified).AddTicks(3838), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 2, 12, 22, 50, 119, DateTimeKind.Unspecified).AddTicks(3840), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 2, 12, 22, 50, 119, DateTimeKind.Unspecified).AddTicks(3843), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 2, 12, 22, 50, 119, DateTimeKind.Unspecified).AddTicks(3845), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 3, 2, 12, 22, 50, 119, DateTimeKind.Unspecified).AddTicks(3743), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 2, 12, 22, 50, 119, DateTimeKind.Unspecified).AddTicks(3743), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$RRq38HFM5oFiQ2FU6lX8Ouq5ikzNlz3dEvSsS4jZNQ5A/snT5cQf6");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 3, 2, 12, 22, 50, 119, DateTimeKind.Unspecified).AddTicks(3816), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 2, 12, 22, 50, 119, DateTimeKind.Unspecified).AddTicks(3816), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
