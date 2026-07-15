using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_API.Migrations
{
    /// <inheritdoc />
    public partial class AddProductUnitsAndBarcodes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BaseQuantity",
                schema: "dbo",
                table: "SaleLines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProductUnitId",
                schema: "dbo",
                table: "SaleLines",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitFactor",
                schema: "dbo",
                table: "SaleLines",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UnitName",
                schema: "dbo",
                table: "SaleLines",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ProductUnits",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Factor = table.Column<int>(type: "int", nullable: false),
                    SellingPrice = table.Column<double>(type: "float", nullable: false),
                    IsBase = table.Column<bool>(type: "bit", nullable: false),
                    IsDefaultForSale = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUserId = table.Column<int>(type: "int", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateUserId = table.Column<int>(type: "int", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    RemoveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemoveUserId = table.Column<int>(type: "int", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductUnits_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "dbo",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductBarcodes",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    ProductUnitId = table.Column<int>(type: "int", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUserId = table.Column<int>(type: "int", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateUserId = table.Column<int>(type: "int", nullable: true),
                    IsRemoved = table.Column<bool>(type: "bit", nullable: false),
                    RemoveDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RemoveUserId = table.Column<int>(type: "int", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductBarcodes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductBarcodes_ProductUnits_ProductUnitId",
                        column: x => x.ProductUnitId,
                        principalSchema: "dbo",
                        principalTable: "ProductUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductBarcodes_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "dbo",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_Barcode",
                schema: "dbo",
                table: "ProductBarcodes",
                column: "Barcode");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_ProductId",
                schema: "dbo",
                table: "ProductBarcodes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarcodes_ProductUnitId",
                schema: "dbo",
                table: "ProductBarcodes",
                column: "ProductUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_ProductId_Name",
                schema: "dbo",
                table: "ProductUnits",
                columns: new[] { "ProductId", "Name" });

            // Backfill: every existing product gets base unit "مفرد" + its barcode
            migrationBuilder.Sql("""
                INSERT INTO dbo.ProductUnits
                    (ProductId, Name, Factor, SellingPrice, IsBase, IsDefaultForSale, SortOrder, CreateDate, CreateUserId, IsRemoved)
                SELECT
                    p.Id,
                    N'مفرد',
                    1,
                    p.SellingPrice,
                    1,
                    1,
                    0,
                    p.CreateDate,
                    p.CreateUserId,
                    0
                FROM dbo.Products p
                WHERE p.IsRemoved = 0
                  AND NOT EXISTS (
                      SELECT 1 FROM dbo.ProductUnits u
                      WHERE u.ProductId = p.Id AND u.IsRemoved = 0
                  );

                INSERT INTO dbo.ProductBarcodes
                    (ProductId, ProductUnitId, Barcode, IsPrimary, CreateDate, CreateUserId, IsRemoved)
                SELECT
                    u.ProductId,
                    u.Id,
                    p.Barcode,
                    1,
                    u.CreateDate,
                    u.CreateUserId,
                    0
                FROM dbo.ProductUnits u
                INNER JOIN dbo.Products p ON p.Id = u.ProductId
                WHERE u.IsBase = 1
                  AND u.IsRemoved = 0
                  AND p.Barcode IS NOT NULL
                  AND LTRIM(RTRIM(p.Barcode)) <> ''
                  AND NOT EXISTS (
                      SELECT 1 FROM dbo.ProductBarcodes b
                      WHERE b.ProductUnitId = u.Id AND b.IsRemoved = 0
                  );

                UPDATE dbo.SaleLines
                SET BaseQuantity = Quantity,
                    UnitFactor = 1,
                    UnitName = N'مفرد'
                WHERE BaseQuantity = 0 OR UnitFactor = 0 OR UnitName = '';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductBarcodes",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "ProductUnits",
                schema: "dbo");

            migrationBuilder.DropColumn(
                name: "BaseQuantity",
                schema: "dbo",
                table: "SaleLines");

            migrationBuilder.DropColumn(
                name: "ProductUnitId",
                schema: "dbo",
                table: "SaleLines");

            migrationBuilder.DropColumn(
                name: "UnitFactor",
                schema: "dbo",
                table: "SaleLines");

            migrationBuilder.DropColumn(
                name: "UnitName",
                schema: "dbo",
                table: "SaleLines");
        }
    }
}
