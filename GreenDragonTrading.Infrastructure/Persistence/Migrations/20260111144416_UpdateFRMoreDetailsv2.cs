using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFRMoreDetailsv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "financial_report_values");

            migrationBuilder.DropTable(
                name: "financial_report_items");

            migrationBuilder.DropColumn(
                name: "content_type",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "financial_reports");

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 11, 14, 44, 15, 539, DateTimeKind.Unspecified).AddTicks(7739), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 11, 14, 44, 15, 539, DateTimeKind.Unspecified).AddTicks(7740), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 11, 14, 44, 15, 539, DateTimeKind.Unspecified).AddTicks(7998), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 11, 14, 44, 15, 539, DateTimeKind.Unspecified).AddTicks(8000), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "financial_reports",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "financial_reports",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "financial_report_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category = table.Column<short>(type: "smallint", nullable: false),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    unit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_report_items", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "financial_report_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    value = table.Column<decimal>(type: "numeric(20,4)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financial_report_values", x => x.id);
                    table.ForeignKey(
                        name: "FK_financial_report_values_financial_report_items_item_id",
                        column: x => x.item_id,
                        principalTable: "financial_report_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_financial_report_values_financial_reports_report_id",
                        column: x => x.report_id,
                        principalTable: "financial_reports",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 11, 13, 28, 14, 806, DateTimeKind.Unspecified).AddTicks(8825), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 11, 13, 28, 14, 806, DateTimeKind.Unspecified).AddTicks(8826), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 11, 13, 28, 14, 806, DateTimeKind.Unspecified).AddTicks(8883), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 11, 13, 28, 14, 806, DateTimeKind.Unspecified).AddTicks(8883), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_financial_report_items_category",
                table: "financial_report_items",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_financial_report_items_code",
                table: "financial_report_items",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_financial_report_values_item_id",
                table: "financial_report_values",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_financial_report_values_report_id",
                table: "financial_report_values",
                column: "report_id");

            migrationBuilder.CreateIndex(
                name: "IX_financial_report_values_report_id_item_id",
                table: "financial_report_values",
                columns: new[] { "report_id", "item_id" },
                unique: true);
        }
    }
}
