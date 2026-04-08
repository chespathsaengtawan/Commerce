using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopInstallment.Migrations
{
    /// <inheritdoc />
    public partial class addFkGarder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Categories_GenderId",
                table: "Categories",
                column: "GenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Genders_GenderId",
                table: "Categories",
                column: "GenderId",
                principalTable: "Genders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Genders_GenderId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Categories_GenderId",
                table: "Categories");
        }
    }
}
