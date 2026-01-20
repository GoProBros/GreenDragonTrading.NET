using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusToSectors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "status",
                table: "sectors",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 18, 11, 1, 7, 840, DateTimeKind.Unspecified).AddTicks(9036), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 18, 11, 1, 7, 840, DateTimeKind.Unspecified).AddTicks(9037), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$dUjRftw200w1ubNjeblkguYpHVZShtJms/jHUltSeoYJ7cR0cyNNe");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 18, 11, 1, 7, 840, DateTimeKind.Unspecified).AddTicks(9166), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 18, 11, 1, 7, 840, DateTimeKind.Unspecified).AddTicks(9167), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "sectors");

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 16, 14, 35, 14, 887, DateTimeKind.Unspecified).AddTicks(9184), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 16, 14, 35, 14, 887, DateTimeKind.Unspecified).AddTicks(9186), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$y99atouBMSFU1RGWaKDebegoBTTJDfz9uVZ9G0G3yx0C8OSyjzwt6");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 16, 14, 35, 14, 887, DateTimeKind.Unspecified).AddTicks(9453), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 16, 14, 35, 14, 887, DateTimeKind.Unspecified).AddTicks(9454), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
