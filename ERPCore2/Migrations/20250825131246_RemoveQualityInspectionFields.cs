using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveQualityInspectionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QualityInspectionPassed",
                table: "PurchaseReceivingDetails");

            migrationBuilder.DropColumn(
                name: "QualityRemarks",
                table: "PurchaseReceivingDetails");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "QualityInspectionPassed",
                table: "PurchaseReceivingDetails",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "QualityRemarks",
                table: "PurchaseReceivingDetails",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
