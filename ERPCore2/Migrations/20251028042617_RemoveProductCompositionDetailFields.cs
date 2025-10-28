using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProductCompositionDetailFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductCompositionDetails_ProductCompositionId_Sequence",
                table: "ProductCompositionDetails");

            migrationBuilder.DropColumn(
                name: "IsKeyComponent",
                table: "ProductCompositionDetails");

            migrationBuilder.DropColumn(
                name: "IsOptional",
                table: "ProductCompositionDetails");

            migrationBuilder.DropColumn(
                name: "LossRate",
                table: "ProductCompositionDetails");

            migrationBuilder.DropColumn(
                name: "MaxQuantity",
                table: "ProductCompositionDetails");

            migrationBuilder.DropColumn(
                name: "MinQuantity",
                table: "ProductCompositionDetails");

            migrationBuilder.DropColumn(
                name: "PositionDescription",
                table: "ProductCompositionDetails");

            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "ProductCompositionDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsKeyComponent",
                table: "ProductCompositionDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOptional",
                table: "ProductCompositionDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LossRate",
                table: "ProductCompositionDetails",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxQuantity",
                table: "ProductCompositionDetails",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinQuantity",
                table: "ProductCompositionDetails",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PositionDescription",
                table: "ProductCompositionDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "ProductCompositionDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCompositionDetails_ProductCompositionId_Sequence",
                table: "ProductCompositionDetails",
                columns: new[] { "ProductCompositionId", "Sequence" });
        }
    }
}
