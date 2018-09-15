using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace StockProcessor.Migrations {
    public partial class seed : Migration {
        protected override void Up (MigrationBuilder migrationBuilder) {
            migrationBuilder.EnsureSchema (
                name: "dbs");

            migrationBuilder.CreateTable (
                name: "Stocks",
                schema: "dbs",
                columns : table => new {
                    Id = table.Column<int> (nullable: false)
                        .Annotation ("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                        Symbol = table.Column<string> (type: "varchar(256)", nullable : true),
                        DayOpen = table.Column<decimal> (type: "decimal(10, 2)", nullable : false),
                        DayLow = table.Column<decimal> (type: "decimal(10, 2)", nullable : false),
                        DayHigh = table.Column<decimal> (type: "decimal(10, 2)", nullable : false),
                        LastChange = table.Column<decimal> (type: "decimal(10, 2)", nullable : false),
                        Price = table.Column<decimal> (type: "decimal(10, 2)", nullable : false)
                },
                constraints : table => {
                    table.PrimaryKey ("PK_Stocks", x => x.Id);
                });
        }

        protected override void Down (MigrationBuilder migrationBuilder) {
            migrationBuilder.DropTable (
                name: "Stocks",
                schema: "dbs");
        }
    }
}