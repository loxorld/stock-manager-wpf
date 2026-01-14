using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StockManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "phone_models",
                columns: new[] { "Id", "Brand", "ModelName" },
                values: new object[,]
                {
                    { 1, "Samsung", "A02" },
                    { 2, "Samsung", "A20" }
                });

            migrationBuilder.InsertData(
                table: "skus",
                columns: new[] { "Id", "Active", "CaseType", "Category", "Cost", "Name", "PhoneModelId", "Price", "ProtectorType", "Stock" },
                values: new object[,]
                {
                    { 5, true, null, 3, 4000m, "Cargador 20W USB-C", null, 7500m, null, 6 },
                    { 1, true, 2, 1, 1500m, "Funda silicona Samsung A02", 1, 3000m, null, 10 },
                    { 2, true, 1, 1, 1400m, "Funda transparente Samsung A20", 2, 2800m, null, 7 },
                    { 3, true, null, 2, 1200m, "Templado reforzado Samsung A02", 1, 2500m, 2, 12 },
                    { 4, true, null, 2, 1800m, "Templado anti-espía Samsung A20", 2, 3500m, 3, 5 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "skus",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "skus",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "skus",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "skus",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "skus",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "phone_models",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "phone_models",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
