using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StockDatabase.Migrations
{
    public partial class StockUpdateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdateTime",
                schema: "dbs",
                table: "Stocks",
                type: "datetime2(7)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdateTime",
                schema: "dbs",
                table: "Stocks");
        }
    }
}
