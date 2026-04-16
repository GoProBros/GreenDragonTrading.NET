using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePortfolioTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fee",
                table: "trading_transactions");

            migrationBuilder.DropColumn(
                name: "tax",
                table: "trading_transactions");

            migrationBuilder.AddColumn<decimal>(
                name: "investment_capital",
                table: "users",
                type: "numeric(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "portfolios",
                type: "uuid",
                nullable: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_portfolios_users_UserId1",
                table: "portfolios");

            migrationBuilder.DropIndex(
                name: "IX_portfolios_UserId1",
                table: "portfolios");

            migrationBuilder.DropColumn(
                name: "investment_capital",
                table: "users");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "portfolios");

            migrationBuilder.AddColumn<decimal>(
                name: "fee",
                table: "trading_transactions",
                type: "numeric(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "tax",
                table: "trading_transactions",
                type: "numeric(18,4)",
                nullable: true);
        }
    }
}
