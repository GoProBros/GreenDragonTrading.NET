using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFRMoreDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "current_ratio",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "debt_to_equity",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "eps",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "gross_margin",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "net_margin",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "operating_margin",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "quick_ratio",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "roa",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "roe",
                table: "financial_reports");

            migrationBuilder.RenameColumn(
                name: "total_liabilities",
                table: "financial_reports",
                newName: "total_resources");

            migrationBuilder.RenameColumn(
                name: "short_term_investments",
                table: "financial_reports",
                newName: "short_term_receivables");

            migrationBuilder.RenameColumn(
                name: "operating_profit",
                table: "financial_reports",
                newName: "short_term_liabilities");

            migrationBuilder.RenameColumn(
                name: "operating_expenses",
                table: "financial_reports",
                newName: "short_term_financial_investments");

            migrationBuilder.RenameColumn(
                name: "net_income",
                table: "financial_reports",
                newName: "short_term_assets");

            migrationBuilder.RenameColumn(
                name: "inventory",
                table: "financial_reports",
                newName: "revenue_from_financial_activities");

            migrationBuilder.RenameColumn(
                name: "current_liabilities",
                table: "financial_reports",
                newName: "revenue_deductions");

            migrationBuilder.RenameColumn(
                name: "current_assets",
                table: "financial_reports",
                newName: "provision_for_decline_in_inventory");

            migrationBuilder.AddColumn<decimal>(
                name: "cash_and_cash_equivalents",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "financial_expenses",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "fixed_assets",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "general_and_administration_expenses",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "income_tax_expense",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "inventories",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "liabilities",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "long_term_liabilities",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "long_term_receivables",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "net_profit",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "net_revenue",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "other_expense",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "other_funds",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "other_income",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "other_profit",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "financial_report_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    name_en = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    category = table.Column<short>(type: "smallint", nullable: false),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    unit = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    report_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_id = table.Column<int>(type: "integer", nullable: false),
                    value = table.Column<decimal>(type: "numeric(20,4)", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "financial_report_values");

            migrationBuilder.DropTable(
                name: "financial_report_items");

            migrationBuilder.DropColumn(
                name: "cash_and_cash_equivalents",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "financial_expenses",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "fixed_assets",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "general_and_administration_expenses",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "income_tax_expense",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "inventories",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "liabilities",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "long_term_liabilities",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "long_term_receivables",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "net_profit",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "net_revenue",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "other_expense",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "other_funds",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "other_income",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "other_profit",
                table: "financial_reports");

            migrationBuilder.RenameColumn(
                name: "total_resources",
                table: "financial_reports",
                newName: "total_liabilities");

            migrationBuilder.RenameColumn(
                name: "short_term_receivables",
                table: "financial_reports",
                newName: "short_term_investments");

            migrationBuilder.RenameColumn(
                name: "short_term_liabilities",
                table: "financial_reports",
                newName: "operating_profit");

            migrationBuilder.RenameColumn(
                name: "short_term_financial_investments",
                table: "financial_reports",
                newName: "operating_expenses");

            migrationBuilder.RenameColumn(
                name: "short_term_assets",
                table: "financial_reports",
                newName: "net_income");

            migrationBuilder.RenameColumn(
                name: "revenue_from_financial_activities",
                table: "financial_reports",
                newName: "inventory");

            migrationBuilder.RenameColumn(
                name: "revenue_deductions",
                table: "financial_reports",
                newName: "current_liabilities");

            migrationBuilder.RenameColumn(
                name: "provision_for_decline_in_inventory",
                table: "financial_reports",
                newName: "current_assets");

            migrationBuilder.AddColumn<decimal>(
                name: "current_ratio",
                table: "financial_reports",
                type: "numeric(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "debt_to_equity",
                table: "financial_reports",
                type: "numeric(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "eps",
                table: "financial_reports",
                type: "numeric(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "gross_margin",
                table: "financial_reports",
                type: "numeric(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "net_margin",
                table: "financial_reports",
                type: "numeric(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "operating_margin",
                table: "financial_reports",
                type: "numeric(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "quick_ratio",
                table: "financial_reports",
                type: "numeric(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "roa",
                table: "financial_reports",
                type: "numeric(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "roe",
                table: "financial_reports",
                type: "numeric(10,4)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 11, 6, 56, 34, 722, DateTimeKind.Unspecified).AddTicks(9100), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 11, 6, 56, 34, 722, DateTimeKind.Unspecified).AddTicks(9101), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 11, 6, 56, 34, 722, DateTimeKind.Unspecified).AddTicks(9176), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 11, 6, 56, 34, 722, DateTimeKind.Unspecified).AddTicks(9177), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
