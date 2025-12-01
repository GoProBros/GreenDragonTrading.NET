using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "exchanges",
                columns: table => new
                {
                    code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    normal_fluctuation_limit = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    first_day_fluctuation_limit = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    no_rights_fluctuation_limit = table.Column<decimal>(type: "numeric(5,4)", nullable: false),
                    status = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_exchanges", x => x.code);
                });

            migrationBuilder.CreateTable(
                name: "sectors",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    en_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    vi_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    parent_id = table.Column<string>(type: "text", nullable: true),
                    level = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sectors", x => x.id);
                    table.ForeignKey(
                        name: "FK_sectors_sectors_parent_id",
                        column: x => x.parent_id,
                        principalTable: "sectors",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "symbols",
                columns: table => new
                {
                    ticker = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    isin = table.Column<string>(type: "text", nullable: true),
                    en_company_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    vi_company_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    exchange_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    sector_id = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<short>(type: "smallint", nullable: false),
                    founding_date = table.Column<DateTime>(type: "date", nullable: true),
                    listing_date = table.Column<DateTime>(type: "date", nullable: true),
                    charter_capital = table.Column<long>(type: "bigint", nullable: true),
                    first_price = table.Column<long>(type: "bigint", nullable: true),
                    address = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    website = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    telephone = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: true),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    fax = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: true),
                    status = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_symbols", x => x.ticker);
                    table.ForeignKey(
                        name: "FK_symbols_exchanges_exchange_code",
                        column: x => x.exchange_code,
                        principalTable: "exchanges",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_symbols_sectors_sector_id",
                        column: x => x.sector_id,
                        principalTable: "sectors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "exchanges",
                columns: new[] { "code", "first_day_fluctuation_limit", "name", "no_rights_fluctuation_limit", "normal_fluctuation_limit", "status" },
                values: new object[,]
                {
                    { "HNX", 0.30m, "Sở Giao dịch Chứng khoán Hà Nội", 0.30m, 0.10m, (short)1 },
                    { "HSX", 0.20m, "Sở Giao dịch Chứng khoán Thành phố Hồ Chí Minh", 0.20m, 0.07m, (short)1 },
                    { "UPCOM", 0.40m, "Thị trường UPCoM", 0.40m, 0.15m, (short)1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_sectors_parent_id",
                table: "sectors",
                column: "parent_id");

            migrationBuilder.CreateIndex(
                name: "IX_symbols_exchange_code",
                table: "symbols",
                column: "exchange_code");

            migrationBuilder.CreateIndex(
                name: "IX_symbols_sector_id",
                table: "symbols",
                column: "sector_id");

            migrationBuilder.CreateIndex(
                name: "IX_symbols_ticker",
                table: "symbols",
                column: "ticker",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_symbols_vi_company_name",
                table: "symbols",
                column: "vi_company_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "symbols");

            migrationBuilder.DropTable(
                name: "exchanges");

            migrationBuilder.DropTable(
                name: "sectors");
        }
    }
}
