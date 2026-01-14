using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StockManager.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "phone_models",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Brand = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    ModelName = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phone_models", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "skus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Category = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 120, nullable: false),
                    PhoneModelId = table.Column<int>(type: "INTEGER", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_skus_phone_models_PhoneModelId",
                        column: x => x.PhoneModelId,
                        principalTable: "phone_models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_phone_models_Brand_ModelName",
                table: "phone_models",
                columns: new[] { "Brand", "ModelName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_skus_PhoneModelId",
                table: "skus",
                column: "PhoneModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "skus");

            migrationBuilder.DropTable(
                name: "phone_models");
        }
    }
}
