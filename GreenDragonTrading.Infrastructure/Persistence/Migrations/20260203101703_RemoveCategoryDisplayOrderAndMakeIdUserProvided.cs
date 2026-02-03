using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCategoryDisplayOrderAndMakeIdUserProvided : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_analysis_report_categories_display_order",
                table: "analysis_report_categories");

            migrationBuilder.DropColumn(
                name: "display_order",
                table: "analysis_report_categories");

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 2, 3, 10, 17, 2, 415, DateTimeKind.Unspecified).AddTicks(8339), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 2, 3, 10, 17, 2, 415, DateTimeKind.Unspecified).AddTicks(8478), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 2, 3, 10, 17, 2, 415, DateTimeKind.Unspecified).AddTicks(8482), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 2, 3, 10, 17, 2, 415, DateTimeKind.Unspecified).AddTicks(8485), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 2, 3, 10, 17, 2, 415, DateTimeKind.Unspecified).AddTicks(8487), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 3, 10, 17, 2, 415, DateTimeKind.Unspecified).AddTicks(8157), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 3, 10, 17, 2, 415, DateTimeKind.Unspecified).AddTicks(8157), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$bJIFeJdLDxpR0rGo3qv/6OKIXqxhBm18f/1RASJ.T45JxT8URyTrK");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 3, 10, 17, 2, 415, DateTimeKind.Unspecified).AddTicks(8310), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 3, 10, 17, 2, 415, DateTimeKind.Unspecified).AddTicks(8311), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "display_order",
                table: "analysis_report_categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                columns: new[] { "created_at", "display_order" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5750), new TimeSpan(0, 0, 0, 0, 0)), 0 });

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                columns: new[] { "created_at", "display_order" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5753), new TimeSpan(0, 0, 0, 0, 0)), 0 });

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                columns: new[] { "created_at", "display_order" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5756), new TimeSpan(0, 0, 0, 0, 0)), 0 });

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                columns: new[] { "created_at", "display_order" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5758), new TimeSpan(0, 0, 0, 0, 0)), 0 });

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                columns: new[] { "created_at", "display_order" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 3, 8, 17, 7, 520, DateTimeKind.Unspecified).AddTicks(5760), new TimeSpan(0, 0, 0, 0, 0)), 0 });

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
        }
    }
}
