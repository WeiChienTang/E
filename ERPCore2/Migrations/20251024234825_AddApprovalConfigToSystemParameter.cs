using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddApprovalConfigToSystemParameter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EnableInventoryTransferApproval",
                table: "SystemParameters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnablePurchaseOrderApproval",
                table: "SystemParameters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnablePurchaseReceivingApproval",
                table: "SystemParameters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnablePurchaseReturnApproval",
                table: "SystemParameters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableSalesOrderApproval",
                table: "SystemParameters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EnableSalesReturnApproval",
                table: "SystemParameters",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnableInventoryTransferApproval",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "EnablePurchaseOrderApproval",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "EnablePurchaseReceivingApproval",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "EnablePurchaseReturnApproval",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "EnableSalesOrderApproval",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "EnableSalesReturnApproval",
                table: "SystemParameters");
        }
    }
}
