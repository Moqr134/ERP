using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_API.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiWarehouseStockAndTransfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Ensure a default warehouse exists for backfill
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM dbo.Warehouses WHERE IsRemoved = 0)
                BEGIN
                    INSERT INTO dbo.Warehouses (Name, Code, Location, PhoneNumber, IsActive, Notes, CreateDate, IsRemoved)
                    VALUES (N'المخزن الرئيسي', N'MAIN', NULL, NULL, 1, N'مخزن افتراضي للترحيل', DATEADD(HOUR, 3, GETUTCDATE()), 0);
                END
                ELSE IF NOT EXISTS (SELECT 1 FROM dbo.Warehouses WHERE Code = N'MAIN' AND IsRemoved = 0)
                BEGIN
                    -- keep existing warehouses; MAIN code is optional when others exist
                    SELECT 1;
                END
                """);

            migrationBuilder.AddColumn<int>(
                name: "RelatedWarehouseId",
                schema: "dbo",
                table: "StockTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                schema: "dbo",
                table: "StockTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                schema: "dbo",
                table: "Sales",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "StockTransfers",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransferNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FromWarehouseId = table.Column<int>(type: "int", nullable: false),
                    ToWarehouseId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_StockTransfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransfers_Warehouses_FromWarehouseId",
                        column: x => x.FromWarehouseId,
                        principalSchema: "dbo",
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransfers_Warehouses_ToWarehouseId",
                        column: x => x.ToWarehouseId,
                        principalSchema: "dbo",
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarehouseStocks",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    WarehouseId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_WarehouseStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WarehouseStocks_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "dbo",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WarehouseStocks_Warehouses_WarehouseId",
                        column: x => x.WarehouseId,
                        principalSchema: "dbo",
                        principalTable: "Warehouses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StockTransferLines",
                schema: "dbo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StockTransferId = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateUserId = table.Column<int>(type: "int", nullable: true),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdateUserId = table.Column<int>(type: "int", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockTransferLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StockTransferLines_Products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "dbo",
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StockTransferLines_StockTransfers_StockTransferId",
                        column: x => x.StockTransferId,
                        principalSchema: "dbo",
                        principalTable: "StockTransfers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Backfill balances + historical warehouse FKs
            migrationBuilder.Sql("""
                DECLARE @DefaultWh int =
                    (SELECT TOP 1 Id FROM dbo.Warehouses WHERE IsRemoved = 0 AND Code = N'MAIN' ORDER BY Id);
                IF @DefaultWh IS NULL
                    SET @DefaultWh = (SELECT TOP 1 Id FROM dbo.Warehouses WHERE IsRemoved = 0 ORDER BY Id);

                IF @DefaultWh IS NULL
                    THROW 50001, N'لا يوجد مخزن لترحيل الأرصدة', 1;

                INSERT INTO dbo.WarehouseStocks (ProductId, WarehouseId, Quantity, CreateDate, IsRemoved)
                SELECT
                    p.Id,
                    COALESCE(p.WarehouseId, @DefaultWh),
                    p.CurrentStock,
                    DATEADD(HOUR, 3, GETUTCDATE()),
                    0
                FROM dbo.Products p
                WHERE p.IsRemoved = 0
                  AND p.CurrentStock <> 0
                  AND NOT EXISTS (
                      SELECT 1 FROM dbo.WarehouseStocks ws
                      WHERE ws.ProductId = p.Id
                        AND ws.WarehouseId = COALESCE(p.WarehouseId, @DefaultWh)
                        AND ws.IsRemoved = 0);

                UPDATE st
                SET st.WarehouseId = COALESCE(p.WarehouseId, @DefaultWh)
                FROM dbo.StockTransactions st
                LEFT JOIN dbo.Products p ON p.Id = st.ProductId
                WHERE st.WarehouseId IS NULL;

                UPDATE dbo.StockTransactions
                SET WarehouseId = @DefaultWh
                WHERE WarehouseId IS NULL;

                UPDATE s
                SET s.WarehouseId = @DefaultWh
                FROM dbo.Sales s
                WHERE s.WarehouseId IS NULL;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseId",
                schema: "dbo",
                table: "StockTransactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "WarehouseId",
                schema: "dbo",
                table: "Sales",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_RelatedWarehouseId",
                schema: "dbo",
                table: "StockTransactions",
                column: "RelatedWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransactions_WarehouseId",
                schema: "dbo",
                table: "StockTransactions",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_Sales_WarehouseId",
                schema: "dbo",
                table: "Sales",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLines_ProductId",
                schema: "dbo",
                table: "StockTransferLines",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferLines_StockTransferId",
                schema: "dbo",
                table: "StockTransferLines",
                column: "StockTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_FromWarehouseId",
                schema: "dbo",
                table: "StockTransfers",
                column: "FromWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_ToWarehouseId",
                schema: "dbo",
                table: "StockTransfers",
                column: "ToWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_TransferNumber",
                schema: "dbo",
                table: "StockTransfers",
                column: "TransferNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseStocks_ProductId_WarehouseId",
                schema: "dbo",
                table: "WarehouseStocks",
                columns: new[] { "ProductId", "WarehouseId" },
                unique: true,
                filter: "[IsRemoved] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseStocks_WarehouseId",
                schema: "dbo",
                table: "WarehouseStocks",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sales_Warehouses_WarehouseId",
                schema: "dbo",
                table: "Sales",
                column: "WarehouseId",
                principalSchema: "dbo",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Warehouses_RelatedWarehouseId",
                schema: "dbo",
                table: "StockTransactions",
                column: "RelatedWarehouseId",
                principalSchema: "dbo",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransactions_Warehouses_WarehouseId",
                schema: "dbo",
                table: "StockTransactions",
                column: "WarehouseId",
                principalSchema: "dbo",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sales_Warehouses_WarehouseId",
                schema: "dbo",
                table: "Sales");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Warehouses_RelatedWarehouseId",
                schema: "dbo",
                table: "StockTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransactions_Warehouses_WarehouseId",
                schema: "dbo",
                table: "StockTransactions");

            migrationBuilder.DropTable(
                name: "StockTransferLines",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "WarehouseStocks",
                schema: "dbo");

            migrationBuilder.DropTable(
                name: "StockTransfers",
                schema: "dbo");

            migrationBuilder.DropIndex(
                name: "IX_StockTransactions_RelatedWarehouseId",
                schema: "dbo",
                table: "StockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_StockTransactions_WarehouseId",
                schema: "dbo",
                table: "StockTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Sales_WarehouseId",
                schema: "dbo",
                table: "Sales");

            migrationBuilder.DropColumn(
                name: "RelatedWarehouseId",
                schema: "dbo",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                schema: "dbo",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                schema: "dbo",
                table: "Sales");
        }
    }
}
