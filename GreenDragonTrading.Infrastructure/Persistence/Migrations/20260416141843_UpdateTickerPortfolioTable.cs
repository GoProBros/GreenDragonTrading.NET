using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTickerPortfolioTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_portfolios_users_UserId1",
                table: "portfolios");

            migrationBuilder.DropForeignKey(
                name: "FK_trading_transactions_symbols_ticker",
                table: "trading_transactions");

            migrationBuilder.DropIndex(
                name: "IX_trading_transactions_ticker",
                table: "trading_transactions");

            migrationBuilder.DropIndex(
                name: "IX_portfolios_UserId1",
                table: "portfolios");

            migrationBuilder.DropColumn(
                name: "ticker",
                table: "trading_transactions");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "portfolios");

            migrationBuilder.AddColumn<string>(
                name: "ticker",
                table: "portfolios",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_portfolios_ticker",
                table: "portfolios",
                column: "ticker");

            migrationBuilder.AddForeignKey(
                name: "FK_portfolios_symbols_ticker",
                table: "portfolios",
                column: "ticker",
                principalTable: "symbols",
                principalColumn: "ticker",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_portfolios_symbols_ticker",
                table: "portfolios");

            migrationBuilder.DropIndex(
                name: "IX_portfolios_ticker",
                table: "portfolios");

            migrationBuilder.DropColumn(
                name: "ticker",
                table: "portfolios");

            migrationBuilder.AddColumn<string>(
                name: "ticker",
                table: "trading_transactions",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "portfolios",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_trading_transactions_ticker",
                table: "trading_transactions",
                column: "ticker");

            migrationBuilder.CreateIndex(
                name: "IX_portfolios_UserId1",
                table: "portfolios",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_portfolios_users_UserId1",
                table: "portfolios",
                column: "UserId1",
                principalTable: "users",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_trading_transactions_symbols_ticker",
                table: "trading_transactions",
                column: "ticker",
                principalTable: "symbols",
                principalColumn: "ticker",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
