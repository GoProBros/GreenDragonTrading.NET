using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProactiveAlertLayerBSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "proactive_alert_layer_b_settings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    timeframe = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    min_absolute_move_percent = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    atr_move_multiplier = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    min_volume_ratio = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    min_adx = table.Column<decimal>(type: "numeric(18,4)", nullable: false),
                    max_indicator_snapshot_age_minutes = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proactive_alert_layer_b_settings", x => x.id);
                });

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 5, 5, 17, 5, 57, 223, DateTimeKind.Unspecified).AddTicks(861), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 5, 5, 17, 5, 57, 223, DateTimeKind.Unspecified).AddTicks(885), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 5, 5, 17, 5, 57, 223, DateTimeKind.Unspecified).AddTicks(888), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 5, 5, 17, 5, 57, 223, DateTimeKind.Unspecified).AddTicks(891), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 5, 5, 17, 5, 57, 223, DateTimeKind.Unspecified).AddTicks(894), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 5, 5, 17, 5, 57, 223, DateTimeKind.Unspecified).AddTicks(368), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 5, 5, 17, 5, 57, 223, DateTimeKind.Unspecified).AddTicks(369), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                table: "proactive_alert_layer_b_settings",
                columns: new[] { "id", "atr_move_multiplier", "created_at", "max_indicator_snapshot_age_minutes", "min_absolute_move_percent", "min_adx", "min_volume_ratio", "timeframe", "updated_at" },
                values: new object[] { 1, 0.7m, new DateTimeOffset(new DateTime(2026, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 2880, 1.0m, 18m, 1.8m, "D1", new DateTimeOffset(new DateTime(2026, 5, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$/w6YfMorUBgYEJfH0FiMueSat16liHXV6pM/m/irLdVuY.W9w1V7a");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "proactive_alert_layer_b_settings");

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "1000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 5, 4, 18, 4, 36, 880, DateTimeKind.Unspecified).AddTicks(8131), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "2000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 5, 4, 18, 4, 36, 880, DateTimeKind.Unspecified).AddTicks(8137), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "3000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 5, 4, 18, 4, 36, 880, DateTimeKind.Unspecified).AddTicks(8141), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "4000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 5, 4, 18, 4, 36, 880, DateTimeKind.Unspecified).AddTicks(8143), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "analysis_report_categories",
                keyColumn: "id",
                keyValue: "5000",
                column: "created_at",
                value: new DateTimeOffset(new DateTime(2026, 5, 4, 18, 4, 36, 880, DateTimeKind.Unspecified).AddTicks(8146), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 5, 4, 18, 4, 36, 880, DateTimeKind.Unspecified).AddTicks(7776), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 5, 4, 18, 4, 36, 880, DateTimeKind.Unspecified).AddTicks(7777), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$K.NxmnGS90PI9uJvigRP8.pfEPUr2PccWoLU3wCYs7mAK0b9rbUbS");
        }
    }
}
