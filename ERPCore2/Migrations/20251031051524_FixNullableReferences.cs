using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class FixNullableReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConvertedQuantity",
                table: "QuotationDetails");

            migrationBuilder.DropColumn(
                name: "LeadTimeDays",
                table: "QuotationDetails");

            migrationBuilder.DropColumn(
                name: "ProductDescription",
                table: "QuotationDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ConvertedQuantity",
                table: "QuotationDetails",
                type: "decimal(18,3)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "LeadTimeDays",
                table: "QuotationDetails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductDescription",
                table: "QuotationDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
