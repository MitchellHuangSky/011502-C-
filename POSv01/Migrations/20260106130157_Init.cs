using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace POSv01.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Barcode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Barcode", "CreatedAt", "IsActive", "Name", "UnitPrice" },
                values: new object[,]
                {
                    { 1, "471000000001", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "測試商品A", 35m },
                    { 2, "471000000002", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "測試商品B", 25m },
                    { 3, "471000000003", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "測試商品C", 59m },
                    { 4, "471000000004", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "瓶裝水", 20m },
                    { 5, "471000000005", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "麵包", 45m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Barcode",
                table: "Products",
                column: "Barcode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}
