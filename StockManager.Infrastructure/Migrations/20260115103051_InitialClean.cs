using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StockManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialClean : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "skus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    CaseType = table.Column<int>(type: "INTEGER", nullable: true),
                    ProtectorType = table.Column<int>(type: "INTEGER", nullable: true),
                    Stock = table.Column<int>(type: "INTEGER", nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_skus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "stock_movements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SkuId = table.Column<int>(type: "INTEGER", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    SignedQuantity = table.Column<int>(type: "INTEGER", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: true),
                    UnitCost = table.Column<decimal>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_movements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stock_movements_skus_SkuId",
                        column: x => x.SkuId,
                        principalTable: "skus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "skus",
                columns: new[] { "Id", "Active", "CaseType", "Category", "Cost", "Name", "Price", "ProtectorType", "Stock" },
                values: new object[,]
                {
                    { 1, true, 2, 1, 1500m, "Funda silicona Samsung A02", 3000m, null, 10 },
                    { 2, true, 1, 1, 1400m, "Funda transparente Samsung A20", 2800m, null, 7 },
                    { 3, true, null, 2, 1200m, "Templado reforzado Samsung A02", 2500m, 2, 12 },
                    { 4, true, null, 2, 1800m, "Templado anti-espía Samsung A20", 3500m, 3, 5 },
                    { 5, true, null, 3, 4000m, "Cargador 20W USB-C", 7500m, null, 6 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_CreatedAt",
                table: "stock_movements",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movements_SkuId",
                table: "stock_movements",
                column: "SkuId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_movements");

            migrationBuilder.DropTable(
                name: "skus");
        }
    }
}
