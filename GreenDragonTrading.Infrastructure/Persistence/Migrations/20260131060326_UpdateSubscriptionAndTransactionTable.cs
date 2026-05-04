using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubscriptionAndTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "max_layouts",
                table: "subscriptions",
                newName: "max_workspaces");

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "transactions",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "transaction_type",
                table: "transactions",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<string>(
                name: "allowed_modules",
                table: "subscriptions",
                type: "jsonb",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 31, 6, 3, 25, 293, DateTimeKind.Unspecified).AddTicks(3498), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 31, 6, 3, 25, 293, DateTimeKind.Unspecified).AddTicks(3499), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "transaction_type",
                table: "transactions");

            migrationBuilder.DropColumn(
                name: "allowed_modules",
                table: "subscriptions");

            migrationBuilder.RenameColumn(
                name: "max_workspaces",
                table: "subscriptions",
                newName: "max_layouts");

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 18, 14, 6, 35, 107, DateTimeKind.Unspecified).AddTicks(5327), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 18, 14, 6, 35, 107, DateTimeKind.Unspecified).AddTicks(5327), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
