using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddResponseDataMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "error_details",
                table: "chat_messages",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "response_data",
                table: "chat_messages",
                type: "jsonb",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 14, 5, 0, 31, 823, DateTimeKind.Unspecified).AddTicks(279), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 14, 5, 0, 31, 823, DateTimeKind.Unspecified).AddTicks(302), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 14, 5, 0, 31, 823, DateTimeKind.Unspecified).AddTicks(304), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 14, 5, 0, 31, 823, DateTimeKind.Unspecified).AddTicks(307), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 14, 5, 0, 31, 823, DateTimeKind.Unspecified).AddTicks(309), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 3, 14, 5, 0, 31, 823, DateTimeKind.Unspecified).AddTicks(173), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 14, 5, 0, 31, 823, DateTimeKind.Unspecified).AddTicks(174), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$Ciu2LykwaE2BHHMFqn10Pu3YlR4z.mxwGyFc1PdhT0IEdI4aldbKW");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 3, 14, 5, 0, 31, 823, DateTimeKind.Unspecified).AddTicks(250), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 14, 5, 0, 31, 823, DateTimeKind.Unspecified).AddTicks(251), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "error_details",
                table: "chat_messages");

            migrationBuilder.DropColumn(
                name: "response_data",
                table: "chat_messages");

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 14, 4, 18, 17, 739, DateTimeKind.Unspecified).AddTicks(8130), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 14, 4, 18, 17, 739, DateTimeKind.Unspecified).AddTicks(8147), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 14, 4, 18, 17, 739, DateTimeKind.Unspecified).AddTicks(8150), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 14, 4, 18, 17, 739, DateTimeKind.Unspecified).AddTicks(8152), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 3, 14, 4, 18, 17, 739, DateTimeKind.Unspecified).AddTicks(8155), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 3, 14, 4, 18, 17, 739, DateTimeKind.Unspecified).AddTicks(8002), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 14, 4, 18, 17, 739, DateTimeKind.Unspecified).AddTicks(8003), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$RKk7iqxVqZsaAQIhl8xBkO9pKh9blXA9t5eaBT0rSwDHCZLIAsGs.");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 3, 14, 4, 18, 17, 739, DateTimeKind.Unspecified).AddTicks(8095), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 14, 4, 18, 17, 739, DateTimeKind.Unspecified).AddTicks(8095), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
