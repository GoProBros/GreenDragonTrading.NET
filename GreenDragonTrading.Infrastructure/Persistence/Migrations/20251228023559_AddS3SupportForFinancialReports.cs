using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddS3SupportForFinancialReports : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("13b9f64a-7143-46ca-8e9b-016f6ec3c316"));

            migrationBuilder.DropColumn(
                name: "original_file_name",
                table: "financial_reports");

            migrationBuilder.AddColumn<string>(
                name: "content_type",
                table: "financial_reports",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "file_path",
                table: "financial_reports",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "file_size",
                table: "financial_reports",
                type: "bigint",
                nullable: true);

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "avatar_url", "created_at", "email", "hashed_password", "is_email_verified", "phone_number", "role", "status", "username" },
                values: new object[] { new Guid("d0209dcd-e934-4969-9bdb-6e9d8fdc71ba"), null, new DateTimeOffset(new DateTime(2025, 12, 28, 2, 35, 59, 24, DateTimeKind.Unspecified).AddTicks(5427), new TimeSpan(0, 0, 0, 0, 0)), "greendragon.trading.team@gmail.com", "96cae35ce8a9b0244178bf28e4966c2ce1b8385723a96a6b838858cdd6ca0a1e", true, "0988671875", (short)3, (short)1, "admin" });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 12, 28, 2, 35, 59, 24, DateTimeKind.Unspecified).AddTicks(5645), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 12, 28, 2, 35, 59, 24, DateTimeKind.Unspecified).AddTicks(5645), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_financial_reports_file_path",
                table: "financial_reports",
                column: "file_path");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_financial_reports_file_path",
                table: "financial_reports");

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("d0209dcd-e934-4969-9bdb-6e9d8fdc71ba"));

            migrationBuilder.DropColumn(
                name: "content_type",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "file_path",
                table: "financial_reports");

            migrationBuilder.DropColumn(
                name: "file_size",
                table: "financial_reports");

            migrationBuilder.AddColumn<string>(
                name: "original_file_name",
                table: "financial_reports",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "avatar_url", "created_at", "email", "hashed_password", "is_email_verified", "phone_number", "role", "status", "username" },
                values: new object[] { new Guid("13b9f64a-7143-46ca-8e9b-016f6ec3c316"), null, new DateTimeOffset(new DateTime(2025, 12, 27, 7, 45, 31, 131, DateTimeKind.Unspecified).AddTicks(6465), new TimeSpan(0, 0, 0, 0, 0)), "greendragon.trading.team@gmail.com", "96cae35ce8a9b0244178bf28e4966c2ce1b8385723a96a6b838858cdd6ca0a1e", true, "0988671875", (short)3, (short)1, "admin" });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 12, 27, 7, 45, 31, 131, DateTimeKind.Unspecified).AddTicks(6599), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 12, 27, 7, 45, 31, 131, DateTimeKind.Unspecified).AddTicks(6600), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
