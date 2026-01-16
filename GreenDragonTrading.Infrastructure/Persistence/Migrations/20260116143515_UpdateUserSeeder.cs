using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenDragonTrading.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserSeeder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var bcryptHash = "$2a$11$YsxugJFUGAbCrao4lEbcFuUGg/3IWZ4CirYM38WPUDjXeHpMkizK6";
            var oldId = "11111111-1111-1111-1111-111111111111";
            var newId = "4c0aa1c2-bece-4999-a020-7cb8dc638cef";

            migrationBuilder.Sql(@$"
                UPDATE users 
                SET id = '{newId}', 
                    hashed_password = '{bcryptHash}', 
                    username = 'Admin'
                WHERE id = '{oldId}';
            ");

            migrationBuilder.Sql(@$"
                UPDATE users 
                SET hashed_password = '{bcryptHash}', 
                    username = 'Admin'
                WHERE id = '{newId}' AND hashed_password NOT LIKE '$2a$%';
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
