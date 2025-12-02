using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSymbolRemoveUnnecessary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "address",
                table: "symbols");

            migrationBuilder.DropColumn(
                name: "charter_capital",
                table: "symbols");

            migrationBuilder.DropColumn(
                name: "company_profile",
                table: "symbols");

            migrationBuilder.DropColumn(
                name: "email",
                table: "symbols");

            migrationBuilder.DropColumn(
                name: "fax",
                table: "symbols");

            migrationBuilder.DropColumn(
                name: "first_price",
                table: "symbols");

            migrationBuilder.DropColumn(
                name: "founding_date",
                table: "symbols");

            migrationBuilder.DropColumn(
                name: "listing_date",
                table: "symbols");

            migrationBuilder.DropColumn(
                name: "telephone",
                table: "symbols");

            migrationBuilder.DropColumn(
                name: "website",
                table: "symbols");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "address",
                table: "symbols",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "charter_capital",
                table: "symbols",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "company_profile",
                table: "symbols",
                type: "varchar",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "symbols",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "fax",
                table: "symbols",
                type: "varchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "first_price",
                table: "symbols",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "founding_date",
                table: "symbols",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "listing_date",
                table: "symbols",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "telephone",
                table: "symbols",
                type: "varchar(25)",
                maxLength: 25,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "website",
                table: "symbols",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
