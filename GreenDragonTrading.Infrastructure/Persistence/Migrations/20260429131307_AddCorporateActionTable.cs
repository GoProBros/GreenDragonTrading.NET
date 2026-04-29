using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCorporateActionTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "corporate_actions",
                columns: table => new
                {
                    event_id = table.Column<int>(type: "integer", nullable: false),
                    ticker = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    title = table.Column<string>(type: "text", nullable: true),
                    title_event = table.Column<string>(type: "text", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    url = table.Column<string>(type: "varchar(500)", nullable: true),
                    ex_rights_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    record_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    action_date = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    event_type = table.Column<short>(type: "smallint", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_corporate_actions", x => x.event_id);
                    table.ForeignKey(
                        name: "FK_corporate_actions_symbols_ticker",
                        column: x => x.ticker,
                        principalTable: "symbols",
                        principalColumn: "ticker",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_corporate_actions_ticker",
                table: "corporate_actions",
                column: "ticker");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "corporate_actions");
        }
    }
}
