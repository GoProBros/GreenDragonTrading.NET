using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSymbolRemoveIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_symbols_ticker",
                table: "symbols");

            migrationBuilder.DropIndex(
                name: "IX_symbols_vi_company_name",
                table: "symbols");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
