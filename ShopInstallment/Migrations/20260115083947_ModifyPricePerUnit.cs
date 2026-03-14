using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopInstallment.Migrations
{
    /// <inheritdoc />
    public partial class ModifyPricePerUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Price",
                table: "Products",
                newName: "PricePerUnit");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PricePerUnit",
                table: "Products",
                newName: "Price");
        }
    }
}
