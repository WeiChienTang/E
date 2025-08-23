using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class RemoveInspectionPersonnelFromPurchaseReceiving : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InspectionPersonnel",
                table: "PurchaseReceivings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InspectionPersonnel",
                table: "PurchaseReceivings",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
