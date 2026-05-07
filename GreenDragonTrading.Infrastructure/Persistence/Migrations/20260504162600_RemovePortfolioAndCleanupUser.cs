using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemovePortfolioAndCleanupUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "trading_transactions");

            migrationBuilder.DropTable(
                name: "portfolios");

            migrationBuilder.DropColumn(
                name: "investment_capital",
                table: "users");

            migrationBuilder.AddColumn<bool>(
                name: "is_admin",
                table: "subscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "is_free",
                table: "subscriptions",
                type: "boolean",
                nullable: false,
                defaultValue: false);   

            migrationBuilder.CreateIndex(
                name: "IX_subscriptions_level_order",
                table: "subscriptions",
                column: "level_order");

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_unique_admin_active",
                table: "subscriptions",
                column: "is_admin",
                unique: true,
                filter: "is_admin = true AND is_active = 1");

            migrationBuilder.CreateIndex(
                name: "ix_subscriptions_unique_free_active",
                table: "subscriptions",
                column: "is_free",
                unique: true,
                filter: "is_free = true AND is_active = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_subscriptions_level_order",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_subscriptions_unique_admin_active",
                table: "subscriptions");

            migrationBuilder.DropIndex(
                name: "ix_subscriptions_unique_free_active",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "is_admin",
                table: "subscriptions");

            migrationBuilder.DropColumn(
                name: "is_free",
                table: "subscriptions");

            migrationBuilder.AddColumn<decimal>(
                name: "investment_capital",
                table: "users",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "portfolios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ticker = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_portfolios", x => x.id);
                    table.ForeignKey(
                        name: "FK_portfolios_symbols_ticker",
                        column: x => x.ticker,
                        principalTable: "symbols",
                        principalColumn: "ticker",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_portfolios_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "trading_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    portfolio_id = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    original_message = table.Column<string>(type: "jsonb", nullable: true),
                    price = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    quantity = table.Column<decimal>(type: "numeric(18,4)", nullable: true),
                    recorded_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    transaction_type = table.Column<short>(type: "smallint", nullable: false),
                    transaction_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trading_transactions", x => x.id);
                    table.ForeignKey(
                        name: "FK_trading_transactions_portfolios_portfolio_id",
                        column: x => x.portfolio_id,
                        principalTable: "portfolios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });  

            migrationBuilder.CreateIndex(
                name: "IX_portfolios_ticker",
                table: "portfolios",
                column: "ticker");

            migrationBuilder.CreateIndex(
                name: "IX_portfolios_user_id",
                table: "portfolios",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_trading_transactions_portfolio_id",
                table: "trading_transactions",
                column: "portfolio_id");
        }
    }
}
