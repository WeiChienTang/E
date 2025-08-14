using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class ChangeEveryTableCodeNameAccordingToBaseEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Warehouses_WarehouseCode",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseLocations_WarehouseId_LocationCode",
                table: "WarehouseLocations");

            migrationBuilder.DropIndex(
                name: "IX_Units_UnitCode",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_SupplierCode",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Sizes_SizeCode",
                table: "Sizes");

            migrationBuilder.DropIndex(
                name: "IX_Products_ProductCode",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_CategoryCode",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_PermissionCode",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactionTypes_TypeCode",
                table: "InventoryTransactionTypes");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CustomerCode",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "WarehouseCode",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "LocationCode",
                table: "WarehouseLocations");

            migrationBuilder.DropColumn(
                name: "UnitCode",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "SupplierCode",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "SizeCode",
                table: "Sizes");

            migrationBuilder.DropColumn(
                name: "ProductCode",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "CategoryCode",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "PermissionCode",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "TypeCode",
                table: "InventoryTransactionTypes");

            migrationBuilder.DropColumn(
                name: "EmployeeCode",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CustomerCode",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Weathers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Materials",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Colors",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_Code",
                table: "Warehouses",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseLocations_WarehouseId_Code",
                table: "WarehouseLocations",
                columns: new[] { "WarehouseId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Units_Code",
                table: "Units",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_Code",
                table: "Suppliers",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Sizes_Code",
                table: "Sizes",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Products_Code",
                table: "Products",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_Code",
                table: "ProductCategories",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionTypes_Code",
                table: "InventoryTransactionTypes",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Code",
                table: "Employees",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Code",
                table: "Customers",
                column: "Code",
                unique: true,
                filter: "[Code] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Warehouses_Code",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_WarehouseLocations_WarehouseId_Code",
                table: "WarehouseLocations");

            migrationBuilder.DropIndex(
                name: "IX_Units_Code",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Suppliers_Code",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_Sizes_Code",
                table: "Sizes");

            migrationBuilder.DropIndex(
                name: "IX_Products_Code",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductCategories_Code",
                table: "ProductCategories");

            migrationBuilder.DropIndex(
                name: "IX_Permissions_Code",
                table: "Permissions");

            migrationBuilder.DropIndex(
                name: "IX_InventoryTransactionTypes_Code",
                table: "InventoryTransactionTypes");

            migrationBuilder.DropIndex(
                name: "IX_Employees_Code",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Customers_Code",
                table: "Customers");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Weathers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarehouseCode",
                table: "Warehouses",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LocationCode",
                table: "WarehouseLocations",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UnitCode",
                table: "Units",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SupplierCode",
                table: "Suppliers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SizeCode",
                table: "Sizes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductCode",
                table: "Products",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CategoryCode",
                table: "ProductCategories",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PermissionCode",
                table: "Permissions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Materials",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeCode",
                table: "InventoryTransactionTypes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeCode",
                table: "Employees",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CustomerCode",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Colors",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_WarehouseCode",
                table: "Warehouses",
                column: "WarehouseCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarehouseLocations_WarehouseId_LocationCode",
                table: "WarehouseLocations",
                columns: new[] { "WarehouseId", "LocationCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Units_UnitCode",
                table: "Units",
                column: "UnitCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Suppliers_SupplierCode",
                table: "Suppliers",
                column: "SupplierCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sizes_SizeCode",
                table: "Sizes",
                column: "SizeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProductCode",
                table: "Products",
                column: "ProductCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductCategories_CategoryCode",
                table: "ProductCategories",
                column: "CategoryCode",
                unique: true,
                filter: "[CategoryCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_PermissionCode",
                table: "Permissions",
                column: "PermissionCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactionTypes_TypeCode",
                table: "InventoryTransactionTypes",
                column: "TypeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerCode",
                table: "Customers",
                column: "CustomerCode",
                unique: true);
        }
    }
}
