using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTransactionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    order_code = table.Column<long>(type: "bigint", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_id = table.Column<int>(type: "integer", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    status = table.Column<short>(type: "smallint", maxLength: 20, nullable: false, defaultValue: (short)0),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_transactions_subscriptions_subscription_id",
                        column: x => x.subscription_id,
                        principalTable: "subscriptions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transactions_users_user_id",
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
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 18, 14, 6, 35, 107, DateTimeKind.Unspecified).AddTicks(5327), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 18, 14, 6, 35, 107, DateTimeKind.Unspecified).AddTicks(5327), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("4c0aa1c2-bece-4999-a020-7cb8dc638cef"),
                column: "hashed_password",
                value: "$2a$11$fFn1LaR/RMKtXarPBjcQAer//6lpLhChk.JH1UY4rCZHlxGp0ea4a");

            migrationBuilder.UpdateData(
                table: "workspaces",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 1, 18, 14, 6, 35, 107, DateTimeKind.Unspecified).AddTicks(5398), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 1, 18, 14, 6, 35, 107, DateTimeKind.Unspecified).AddTicks(5399), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_transactions_order_code",
                table: "transactions",
                column: "order_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_transactions_subscription_id",
                table: "transactions",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "IX_transactions_user_id",
                table: "transactions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "transactions");

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
