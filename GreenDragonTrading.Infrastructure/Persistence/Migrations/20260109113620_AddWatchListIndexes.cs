using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWatchListIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("f0a0988e-cc8d-406f-9868-b014f5bc4689"));

            migrationBuilder.CreateTable(
                name: "watch_lists",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    tickers = table.Column<string>(type: "jsonb", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_watch_lists", x => x.id);
                    table.ForeignKey(
                        name: "FK_watch_lists_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_watch_lists_user_id",
                table: "watch_lists",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_watch_lists_user_id_name",
                table: "watch_lists",
                columns: new[] { "user_id", "name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "watch_lists");

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("ea87b37e-8200-4439-9596-7f8a5660f631"));

            migrationBuilder.UpdateData(
                table: "module_layouts",
                keyColumn: "id",
                keyValue: 1L,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 12, 28, 6, 33, 13, 356, DateTimeKind.Unspecified).AddTicks(794), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 12, 28, 6, 33, 13, 356, DateTimeKind.Unspecified).AddTicks(794), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "avatar_url", "created_at", "email", "hashed_password", "is_email_verified", "phone_number", "role", "status", "username" },
                values: new object[] { new Guid("f0a0988e-cc8d-406f-9868-b014f5bc4689"), null, new DateTimeOffset(new DateTime(2025, 12, 28, 6, 33, 13, 356, DateTimeKind.Unspecified).AddTicks(754), new TimeSpan(0, 0, 0, 0, 0)), "greendragon.trading.team@gmail.com", "96cae35ce8a9b0244178bf28e4966c2ce1b8385723a96a6b838858cdd6ca0a1e", true, "0988671875", (short)3, (short)1, "admin" });

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2025, 12, 28, 6, 33, 13, 356, DateTimeKind.Unspecified).AddTicks(831), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2025, 12, 28, 6, 33, 13, 356, DateTimeKind.Unspecified).AddTicks(832), new TimeSpan(0, 0, 0, 0, 0)) });
        }
    }
}
