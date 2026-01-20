using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RefactorToProcessAndDiscard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("351469d9-ea39-4327-ae96-3cb462969371"));

            migrationBuilder.DropColumn(
                name: "file_path",
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("13b9f64a-7143-46ca-8e9b-016f6ec3c316"));

            migrationBuilder.DropColumn(
                name: "original_file_name",
                table: "financial_reports");

            migrationBuilder.AddColumn<string>(
                name: "file_path",
                table: "financial_reports",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "avatar_url", "created_at", "email", "hashed_password", "is_email_verified", "phone_number", "role", "status", "username" },
                values: new object[] { new Guid("351469d9-ea39-4327-ae96-3cb462969371"), null, new DateTimeOffset(new DateTime(2025, 12, 27, 7, 33, 26, 490, DateTimeKind.Unspecified).AddTicks(4994), new TimeSpan(0, 0, 0, 0, 0)), "greendragon.trading.team@gmail.com", "96cae35ce8a9b0244178bf28e4966c2ce1b8385723a96a6b838858cdd6ca0a1e", true, "0988671875", (short)3, (short)1, "admin" });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 12, 27, 7, 33, 26, 490, DateTimeKind.Unspecified).AddTicks(5076), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 12, 27, 7, 33, 26, 490, DateTimeKind.Unspecified).AddTicks(5077), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
