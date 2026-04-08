using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddListedShareAndIndicatorData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "listed_share",
                table: "symbols",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "indicator_data",
                table: "financial_reports",
                type: "jsonb",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 7, 16, 14, 7, 300, DateTimeKind.Unspecified).AddTicks(655), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 7, 16, 14, 7, 300, DateTimeKind.Unspecified).AddTicks(675), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 7, 16, 14, 7, 300, DateTimeKind.Unspecified).AddTicks(678), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 7, 16, 14, 7, 300, DateTimeKind.Unspecified).AddTicks(680), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 7, 16, 14, 7, 300, DateTimeKind.Unspecified).AddTicks(682), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 4, 7, 16, 14, 7, 300, DateTimeKind.Unspecified).AddTicks(450), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 4, 7, 16, 14, 7, 300, DateTimeKind.Unspecified).AddTicks(450), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$pj2qN75TtRlqnLZARg82TOkzbWA8lcoJRjxPO4Q7dt4VEJSdcK8KK");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "listed_share",
                table: "symbols");

            migrationBuilder.DropColumn(
                name: "indicator_data",
                table: "financial_reports");

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 5, 11, 50, 24, 947, DateTimeKind.Unspecified).AddTicks(1345), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 5, 11, 50, 24, 947, DateTimeKind.Unspecified).AddTicks(1363), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 5, 11, 50, 24, 947, DateTimeKind.Unspecified).AddTicks(1366), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 5, 11, 50, 24, 947, DateTimeKind.Unspecified).AddTicks(1369), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 4, 5, 11, 50, 24, 947, DateTimeKind.Unspecified).AddTicks(1371), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 4, 5, 11, 50, 24, 947, DateTimeKind.Unspecified).AddTicks(1222), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 4, 5, 11, 50, 24, 947, DateTimeKind.Unspecified).AddTicks(1223), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$kRkG3mGALcnkFutsg8FZTOtPyek/GAxdtVWIwfBP3Boyk.NYvlVtu");
        }
    }
}
