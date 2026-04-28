using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "alert_templates",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    alert_type = table.Column<short>(type: "smallint", nullable: true),
                    condition_type = table.Column<short>(type: "smallint", nullable: true),
                    title_template = table.Column<string>(type: "text", nullable: false),
                    body_template = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alert_templates", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alert_templates_alert_type_condition_type",
                table: "alert_templates",
                columns: new[] { "alert_type", "condition_type" },
                unique: true,
                filter: "is_active = true");

            migrationBuilder.CreateIndex(
                name: "IX_alert_templates_is_default",
                table: "alert_templates",
                column: "is_default",
                unique: true,
                filter: "is_default = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alert_templates");
        }
    }
}
