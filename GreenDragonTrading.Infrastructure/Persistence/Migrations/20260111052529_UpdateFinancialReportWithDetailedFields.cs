using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFinancialReportWithDetailedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("ea87b37e-8200-4439-9596-7f8a5660f631"));

            migrationBuilder.DropColumn(
                name: "key_metrics",
                table: "financial_reports");

            migrationBuilder.AddColumn<decimal>(
                name: "beginning_cash",
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
                name: "current_assets",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "current_liabilities",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

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
                name: "ending_cash",
                table: "financial_reports",
                type: "numeric(20,2)",
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
                name: "gross_profit",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "inventory",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "long_term_assets",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "net_cash_flow",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "net_income",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "net_margin",
                table: "financial_reports",
                type: "numeric(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "notes",
                table: "financial_reports",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "operating_expenses",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "operating_margin",
                table: "financial_reports",
                type: "numeric(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "operating_profit",
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
                name: "quick_ratio",
                table: "financial_reports",
                type: "numeric(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "revenue",
                table: "financial_reports",
                type: "numeric(20,2)",
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

            migrationBuilder.AddColumn<decimal>(
                name: "short_term_investments",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_assets",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "total_liabilities",
                table: "financial_reports",
                type: "numeric(20,2)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 11, 5, 25, 28, 660, DateTimeKind.Unspecified).AddTicks(7957), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 11, 5, 25, 28, 660, DateTimeKind.Unspecified).AddTicks(7958), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "avatar_url", "created_at", "email", "hashed_password", "is_email_verified", "phone_number", "role", "status", "username" },
                values: new object[] { new Guid("e6f64f2f-a0e2-4f87-8285-ad0da90a0124"), null, new DateTimeOffset(new DateTime(2026, 1, 11, 5, 25, 28, 660, DateTimeKind.Unspecified).AddTicks(7812), new TimeSpan(0, 0, 0, 0, 0)), "greendragon.trading.team@gmail.com", "96cae35ce8a9b0244178bf28e4966c2ce1b8385723a96a6b838858cdd6ca0a1e", true, "0988671875", (short)3, (short)1, "admin" });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 11, 5, 25, 28, 660, DateTimeKind.Unspecified).AddTicks(8061), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 11, 5, 25, 28, 660, DateTimeKind.Unspecified).AddTicks(8062), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("e6f64f2f-a0e2-4f87-8285-ad0da90a0124"));

            migrationBuilder.DropColumn(
                name: "beginning_cash",
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
                name: "current_assets",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "current_liabilities",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "current_ratio",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "debt_to_equity",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "ending_cash",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "eps",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "gross_margin",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "gross_profit",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "inventory",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "long_term_assets",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "net_cash_flow",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "net_income",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "net_margin",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "notes",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "operating_expenses",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "operating_margin",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "operating_profit",
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
                name: "quick_ratio",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "revenue",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "roa",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "roe",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "short_term_investments",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "total_assets",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "total_liabilities",
                table: "financial_reports");

            migrationBuilder.AddColumn<string>(
                name: "key_metrics",
                table: "financial_reports",
                type: "jsonb",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 9, 11, 36, 20, 422, DateTimeKind.Unspecified).AddTicks(4906), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 9, 11, 36, 20, 422, DateTimeKind.Unspecified).AddTicks(4907), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "avatar_url", "created_at", "email", "hashed_password", "is_email_verified", "phone_number", "role", "status", "username" },
                values: new object[] { new Guid("ea87b37e-8200-4439-9596-7f8a5660f631"), null, new DateTimeOffset(new DateTime(2026, 1, 9, 11, 36, 20, 422, DateTimeKind.Unspecified).AddTicks(4858), new TimeSpan(0, 0, 0, 0, 0)), "greendragon.trading.team@gmail.com", "96cae35ce8a9b0244178bf28e4966c2ce1b8385723a96a6b838858cdd6ca0a1e", true, "0988671875", (short)3, (short)1, "admin" });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 9, 11, 36, 20, 422, DateTimeKind.Unspecified).AddTicks(4958), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 9, 11, 36, 20, 422, DateTimeKind.Unspecified).AddTicks(4959), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
