using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddCodeToBaseEntityAndRemoveDepartmentCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DepartmentCode",
                table: "Departments");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Warehouses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "WarehouseLocations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Units",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "UnitConversions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "SupplierTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Suppliers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "SupplierPricings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "StockTakings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "StockTakingDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Sizes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "SalesReturns",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "SalesReturnDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "SalesOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "SalesOrderDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "SalesDeliveryDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "SalesDeliveries",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Roles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "RolePermissions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PurchaseReturns",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PurchaseReturnDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PurchaseReceivings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PurchaseReceivingDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PurchaseOrders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PurchaseOrderDetails",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ProductSuppliers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Products",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ProductPricings",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ProductCategories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "PriceHistories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Permissions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "InventoryTransactionTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "InventoryTransactions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "InventoryStocks",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "InventoryReservations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ErrorLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Employees",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Departments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "DeletedRecords",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "CustomerTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ContactTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Contacts",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "AddressTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Addresses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "WarehouseLocations");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "UnitConversions");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SupplierTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SupplierPricings");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "StockTakings");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "StockTakingDetails");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Sizes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SalesDeliveryDetails");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "SalesDeliveries");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PurchaseReturnDetails");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PurchaseReceivingDetails");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PurchaseOrderDetails");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ProductSuppliers");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ProductPricings");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "PriceHistories");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "InventoryTransactionTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "InventoryReservations");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "DeletedRecords");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "CustomerTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ContactTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Contacts");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "AddressTypes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Addresses");

            migrationBuilder.AddColumn<string>(
                name: "DepartmentCode",
                table: "Departments",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");
        }
    }
}
