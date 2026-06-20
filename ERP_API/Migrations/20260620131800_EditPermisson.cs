using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_API.Migrations
{
    /// <inheritdoc />
    public partial class EditPermisson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permation_Users_UserId",
                schema: "dbo",
                table: "Permation");

            migrationBuilder.DropIndex(
                name: "IX_Permation_UserId",
                schema: "dbo",
                table: "Permation");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "dbo",
                table: "Permation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UserId",
                schema: "dbo",
                table: "Permation",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Permation_UserId",
                schema: "dbo",
                table: "Permation",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Permation_Users_UserId",
                schema: "dbo",
                table: "Permation",
                column: "UserId",
                principalSchema: "dbo",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
