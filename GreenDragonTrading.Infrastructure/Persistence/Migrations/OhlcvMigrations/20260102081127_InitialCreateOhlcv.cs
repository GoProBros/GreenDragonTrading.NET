using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations.OhlcvMigrations
{
    /// <inheritdoc />
    public partial class InitialCreateOhlcv : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ohlcv",
                columns: table => new
                {
                    time = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    ticker = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    timeframe = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    open = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    high = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    low = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    close = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    volume = table.Column<long>(type: "bigint", nullable: false),
                    value = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    trades_count = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                    source = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ohlcv", x => new { x.time, x.ticker, x.timeframe });
                });

            migrationBuilder.CreateIndex(
                name: "idx_ohlcv_created_at",
                table: "ohlcv",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_ohlcv_ticker",
                table: "ohlcv",
                column: "ticker");

            migrationBuilder.CreateIndex(
                name: "idx_ohlcv_ticker_tf_time",
                table: "ohlcv",
                columns: new[] { "ticker", "timeframe", "time" });

            migrationBuilder.CreateIndex(
                name: "idx_ohlcv_time",
                table: "ohlcv",
                column: "time",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ohlcv");
        }
    }
}
