using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMaterialUnusedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Materials_Suppliers_SupplierId",
                table: "Materials");

            migrationBuilder.DropIndex(
                name: "IX_Materials_SupplierId",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "Density",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "IsEcoFriendly",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "MeltingPoint",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Materials");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Materials",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Density",
                table: "Materials",
                type: "decimal(10,4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEcoFriendly",
                table: "Materials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MeltingPoint",
                table: "Materials",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Materials",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Materials_SupplierId",
                table: "Materials",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_Materials_Suppliers_SupplierId",
                table: "Materials",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
