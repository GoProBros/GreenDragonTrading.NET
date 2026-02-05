using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueInUsername : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_users_username",
                table: "users");

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 5, 2, 48, 54, 127, DateTimeKind.Unspecified).AddTicks(3869), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 5, 2, 48, 54, 127, DateTimeKind.Unspecified).AddTicks(3869), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$CEPkic/.IdG1on9t.KoY6eb.jPsRhzvBuIA.IXG964g9wEy4AvsjS");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 2, 5, 2, 48, 54, 127, DateTimeKind.Unspecified).AddTicks(3954), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 2, 5, 2, 48, 54, 127, DateTimeKind.Unspecified).AddTicks(3955), new TimeSpan(0, 0, 0, 0, 0)) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 31, 6, 3, 25, 293, DateTimeKind.Unspecified).AddTicks(3498), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 31, 6, 3, 25, 293, DateTimeKind.Unspecified).AddTicks(3499), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$Usr2LG6A.XQNGHN8fcGz8O9p5ncAA23FPVbk/DxV.NGF2runzos5y");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 31, 6, 3, 25, 293, DateTimeKind.Unspecified).AddTicks(3569), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 31, 6, 3, 25, 293, DateTimeKind.Unspecified).AddTicks(3570), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);
        }
    }
}
