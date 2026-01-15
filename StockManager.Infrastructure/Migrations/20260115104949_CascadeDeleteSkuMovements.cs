using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteSkuMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stock_movements_skus_SkuId",
                table: "stock_movements");

            migrationBuilder.AddForeignKey(
                name: "FK_stock_movements_skus_SkuId",
                table: "stock_movements",
                column: "SkuId",
                principalTable: "skus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stock_movements_skus_SkuId",
                table: "stock_movements");

            migrationBuilder.AddForeignKey(
                name: "FK_stock_movements_skus_SkuId",
                table: "stock_movements",
                column: "SkuId",
                principalTable: "skus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
