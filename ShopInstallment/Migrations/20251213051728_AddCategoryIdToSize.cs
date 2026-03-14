using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopInstallment.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIdToSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Sizes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sizes_CategoryId",
                table: "Sizes",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Sizes_Categories_CategoryId",
                table: "Sizes",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sizes_Categories_CategoryId",
                table: "Sizes");

            migrationBuilder.DropIndex(
                name: "IX_Sizes_CategoryId",
                table: "Sizes");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Sizes");
        }
    }
}
