using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "status",
                table: "symbols",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<short>(
                name: "status",
                table: "exchanges",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.UpdateData(
                table: "exchanges",
                keyColumn: "code",
                keyValue: "HNX",
                column: "status",
                value: (short)1);

            migrationBuilder.UpdateData(
                table: "exchanges",
                keyColumn: "code",
                keyValue: "HSX",
                column: "status",
                value: (short)1);

            migrationBuilder.UpdateData(
                table: "exchanges",
                keyColumn: "code",
                keyValue: "UPCOM",
                column: "status",
                value: (short)1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "symbols");

            migrationBuilder.DropColumn(
                name: "status",
                table: "exchanges");
        }
    }
}
