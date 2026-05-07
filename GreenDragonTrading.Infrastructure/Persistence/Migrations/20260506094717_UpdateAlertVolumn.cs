using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAlertVolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "volume_lookback_bars",
                table: "alerts",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "volume_time_frame",
                table: "alerts",
                type: "smallint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "volume_lookback_bars",
                table: "alerts");

            migrationBuilder.DropColumn(
                name: "volume_time_frame",
                table: "alerts");
        }
    }
}
