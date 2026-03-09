using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddBaseEntityIsDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Weathers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "WasteTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "WasteRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "WarehouseLocations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "VehicleTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Vehicles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "VehicleMaintenances",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Units",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "UnitConversions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "TextMessageTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SystemParameters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SupplierVisits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SupplierPricings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "StockTakings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "StockTakingDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "StickyNotes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Sizes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SetoffProductDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SetoffPrepaymentUsages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SetoffPrepayments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SetoffPayments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SetoffDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SalesReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SalesReturnReasons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SalesReturnDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SalesOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SalesOrderDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SalesOrderCompositionDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SalesDeliveryDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "SalesDeliveries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "RolePermissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "ReportPrintConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Quotations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "QuotationDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "QuotationCompositionDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PurchaseReturns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PurchaseReturnReasons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PurchaseReturnDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PurchaseReceivings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PurchaseReceivingDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PurchaseOrders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PurchaseOrderDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "ProductSuppliers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Products",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "ProductionSchedules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "ProductionScheduleItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "ProductionScheduleDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "ProductionScheduleCompletions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "ProductionScheduleAllocations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "ProductCustomers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "ProductCompositions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "ProductCompositionDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "ProductCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PrinterConfigurations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PriceHistories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PrepaymentTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Permissions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PayrollRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PayrollPeriods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PayrollItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PaymentMethods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "PaperSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Materials",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "MaterialIssues",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "MaterialIssueDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "JournalEntryLines",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "JournalEntries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "InventoryTransactionTypes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "InventoryTransactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "InventoryTransactionDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "InventoryStocks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "InventoryStockDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "InventoryReservations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "ErrorLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "EmployeeTrainingRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "EmployeeTools",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "EmployeeSalaries",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "EmployeePreferences",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "EmployeePositions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "EmployeeLicenses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "EmployeeDashboardPanels",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "EmployeeDashboardConfigs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "EmployeeBankAccounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Documents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "DocumentFiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "DocumentCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Departments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "DeletedRecords",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "CustomerVisits",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Customers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Currencies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "CompositionCategories",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "CompanyModules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Companies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Colors",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "CalendarEvents",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "Banks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDraft",
                table: "AccountItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Weathers");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "WasteTypes");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "WasteRecords");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Warehouses");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "WarehouseLocations");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "VehicleTypes");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Vehicles");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "VehicleMaintenances");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Units");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "UnitConversions");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "TextMessageTemplates");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SystemParameters");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SupplierVisits");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SupplierPricings");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "StockTakings");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "StockTakingDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "StickyNotes");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Sizes");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SetoffProductDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SetoffPrepaymentUsages");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SetoffPrepayments");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SetoffPayments");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SetoffDocuments");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SalesReturnReasons");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SalesReturnDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SalesOrders");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SalesOrderDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SalesOrderCompositionDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SalesDeliveryDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "SalesDeliveries");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "ReportPrintConfigurations");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Quotations");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "QuotationDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "QuotationCompositionDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PurchaseReturnReasons");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PurchaseReturnDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PurchaseReceivings");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PurchaseReceivingDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PurchaseOrderDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "ProductSuppliers");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "ProductionSchedules");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "ProductionScheduleItems");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "ProductionScheduleDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "ProductionScheduleCompletions");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "ProductionScheduleAllocations");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "ProductCustomers");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "ProductCompositions");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "ProductCompositionDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "ProductCategories");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PrinterConfigurations");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PriceHistories");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PrepaymentTypes");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Permissions");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PayrollPeriods");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PayrollItems");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PaymentMethods");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "PaperSettings");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "MaterialIssues");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "MaterialIssueDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "InventoryTransactionTypes");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "InventoryTransactionDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "InventoryStocks");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "InventoryStockDetails");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "InventoryReservations");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "ErrorLogs");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "EmployeeTrainingRecords");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "EmployeeTools");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "EmployeeSalaries");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "EmployeePreferences");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "EmployeePositions");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "EmployeeLicenses");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "EmployeeDashboardPanels");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "EmployeeDashboardConfigs");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "EmployeeBankAccounts");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Documents");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "DocumentFiles");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "DocumentCategories");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "DeletedRecords");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "CustomerVisits");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "CompositionCategories");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "CompanyModules");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Colors");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "CalendarEvents");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "IsDraft",
                table: "AccountItems");
        }
    }
}
