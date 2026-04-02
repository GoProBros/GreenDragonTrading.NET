using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CreateNewsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "news_articles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "varchar(500)", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    summary = table.Column<string>(type: "text", nullable: true),
                    link = table.Column<string>(type: "varchar(1000)", nullable: true),
                    thumbnail_url = table.Column<string>(type: "varchar(1000)", nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_news_articles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "article_tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    news_article_id = table.Column<int>(type: "integer", nullable: false),
                    ticker = table.Column<string>(type: "varchar(20)", nullable: false),
                    relevance_score = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    sentiment_score = table.Column<decimal>(type: "numeric(5,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_article_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_article_tags_news_articles_news_article_id",
                        column: x => x.news_article_id,
                        principalTable: "news_articles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_article_tags_symbols_ticker",
                        column: x => x.ticker,
                        principalTable: "symbols",
                        principalColumn: "ticker",
                        onDelete: ReferentialAction.Restrict);
                });
       
            migrationBuilder.CreateIndex(
                name: "IX_article_tags_news_article_id_ticker",
                table: "article_tags",
                columns: new[] { "news_article_id", "ticker" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_article_tags_ticker",
                table: "article_tags",
                column: "ticker");

            migrationBuilder.CreateIndex(
                name: "IX_news_articles_link",
                table: "news_articles",
                column: "link",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_news_articles_published_at",
                table: "news_articles",
                column: "published_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "article_tags");

            migrationBuilder.DropTable(
                name: "news_articles");

        }
    }
}
