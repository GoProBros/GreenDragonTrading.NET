using System;
using GreenDragonTrading.Domain.Entities;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFRjson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "beginning_cash",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "cash_and_cash_equivalents",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "cash_from_financing",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "cash_from_investing",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "cash_from_operating",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "cost_of_goods_sold",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "ending_cash",
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
                name: "gross_profit",
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
                name: "long_term_assets",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "long_term_liabilities",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "long_term_receivables",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "net_cash_flow",
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

            migrationBuilder.DropColumn(
                name: "owner_equity",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "profit_after_tax",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "profit_before_tax",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "provision_for_decline_in_inventory",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "revenue",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "revenue_deductions",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "revenue_from_financial_activities",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "short_term_assets",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "short_term_financial_investments",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "short_term_liabilities",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "short_term_receivables",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "total_assets",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "total_resources",
                table: "financial_reports");

            migrationBuilder.AddColumn<int>(
                name: "quarter",
                table: "financial_reports",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<FinancialReportData>(
                name: "report_data",
                table: "financial_reports",
                type: "jsonb",
                nullable: false);

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 14, 15, 50, 42, 664, DateTimeKind.Unspecified).AddTicks(3273), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 14, 15, 50, 42, 664, DateTimeKind.Unspecified).AddTicks(3273), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 14, 15, 50, 42, 664, DateTimeKind.Unspecified).AddTicks(3327), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 14, 15, 50, 42, 664, DateTimeKind.Unspecified).AddTicks(3328), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "quarter",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "report_data",
                table: "financial_reports");

            migrationBuilder.AddColumn<decimal>(
                name: "beginning_cash",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "cash_and_cash_equivalents",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "cash_from_financing",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "cash_from_investing",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "cash_from_operating",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "cost_of_goods_sold",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ending_cash",
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
                name: "gross_profit",
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
                name: "long_term_assets",
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
                name: "net_cash_flow",
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

            migrationBuilder.AddColumn<decimal>(
                name: "owner_equity",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "profit_after_tax",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "profit_before_tax",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "provision_for_decline_in_inventory",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "revenue",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "revenue_deductions",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "revenue_from_financial_activities",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "short_term_assets",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "short_term_financial_investments",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "short_term_liabilities",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "short_term_receivables",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_assets",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_resources",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

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
    }
}
