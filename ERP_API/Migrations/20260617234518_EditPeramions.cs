using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_API.Migrations
{
    /// <inheritdoc />
    public partial class EditPeramions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permission_Users_UserId",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "IsRemoved",
                schema: "dbo",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "RemoveDate",
                schema: "dbo",
                table: "StockTransactions");

            migrationBuilder.DropColumn(
                name: "RemoveUserId",
                schema: "dbo",
                table: "StockTransactions");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreateDate",
                schema: "dbo",
                table: "Users",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreateUserId",
                schema: "dbo",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RemoveUserId",
                schema: "dbo",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UpdateUserId",
                schema: "dbo",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Permission",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Permission_Users_UserId",
                table: "Permission",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permission_Users_UserId",
                table: "Permission");

            migrationBuilder.DropColumn(
                name: "CreateDate",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreateUserId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RemoveUserId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdateUserId",
                schema: "dbo",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "IsRemoved",
                schema: "dbo",
                table: "StockTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "RemoveDate",
                schema: "dbo",
                table: "StockTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RemoveUserId",
                schema: "dbo",
                table: "StockTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Permission",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Permission_Users_UserId",
                table: "Permission",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
