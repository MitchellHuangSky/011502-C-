using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace POSv01.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SaleNo = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    Subtotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Total = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    ChangeAmount = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SaleItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SaleId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProductId = table.Column<int>(type: "INTEGER", nullable: false),
                    Barcode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 128, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Quantity = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SaleItems_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleItems_Sales_SaleId",
                        column: x => x.SaleId,
                        principalTable: "Sales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
            /*
            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "Barcode", "CreatedAt", "ImagePath", "IsActive", "Name", "UnitPrice" },
                values: new object[,]
                {
                    { 1, "471000000001", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "測試商品A", 35m },
                    { 2, "471000000002", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "測試商品B", 25m },
                    { 3, "471000000003", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "測試商品C", 59m },
                    { 4, "471000000004", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "瓶裝水", 20m },
                    { 5, "471000000005", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "麵包", 45m }
                });
            */
            migrationBuilder.Sql(@"
INSERT INTO Products (Barcode, CreatedAt, IsActive, Name, UnitPrice)
SELECT '471000000001', '2025-01-01 00:00:00', 1, '測試商品A', 35.0
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Barcode='471000000001');
");

            migrationBuilder.Sql(@"
INSERT INTO Products (Barcode, CreatedAt, IsActive, Name, UnitPrice)
SELECT '471000000002', '2025-01-01 00:00:00', 1, '測試商品B', 25.0
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Barcode='471000000002');
");

            migrationBuilder.Sql(@"
INSERT INTO Products (Barcode, CreatedAt, IsActive, Name, UnitPrice)
SELECT '471000000003', '2025-01-01 00:00:00', 1, '測試商品C', 59.0
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Barcode='471000000003');
");

            migrationBuilder.Sql(@"
INSERT INTO Products (Barcode, CreatedAt, IsActive, Name, UnitPrice)
SELECT '471000000004', '2025-01-01 00:00:00', 1, '瓶裝水', 20.0
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Barcode='471000000004');
");

            migrationBuilder.Sql(@"
INSERT INTO Products (Barcode, CreatedAt, IsActive, Name, UnitPrice)
SELECT '471000000005', '2025-01-01 00:00:00', 1, '麵包', 45.0
WHERE NOT EXISTS (SELECT 1 FROM Products WHERE Barcode='471000000005');
");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_ProductId",
                table: "SaleItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleItems_SaleId",
                table: "SaleItems",
                column: "SaleId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_SaleNo",
                table: "Sales",
                column: "SaleNo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SaleItems");

            migrationBuilder.DropTable(
                name: "Sales");

            migrationBuilder.Sql("DELETE FROM Products WHERE Barcode='471000000001';");
            migrationBuilder.Sql("DELETE FROM Products WHERE Barcode='471000000002';");
            migrationBuilder.Sql("DELETE FROM Products WHERE Barcode='471000000003';");
            migrationBuilder.Sql("DELETE FROM Products WHERE Barcode='471000000004';");
            migrationBuilder.Sql("DELETE FROM Products WHERE Barcode='471000000005';");

            /*
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 5);
            */
        }
    }
}
