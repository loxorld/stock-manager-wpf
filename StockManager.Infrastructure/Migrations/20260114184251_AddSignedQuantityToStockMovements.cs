using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSignedQuantityToStockMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "stock_movements",
                newName: "SignedQuantity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SignedQuantity",
                table: "stock_movements",
                newName: "Quantity");
        }
    }
}
