using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERPCore2.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanyModules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModuleKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyModules", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompanyModules_ModuleKey",
                table: "CompanyModules",
                column: "ModuleKey",
                unique: true);

            // Seed 預設模組（全部啟用）
            var now = new DateTime(2026, 2, 24, 0, 0, 0, DateTimeKind.Local);
            migrationBuilder.InsertData(
                table: "CompanyModules",
                columns: new[] { "ModuleKey", "DisplayName", "Description", "IsEnabled", "SortOrder", "Status", "CreatedAt", "CreatedBy" },
                values: new object[,]
                {
                    { "Customers",           "客戶管理",   "客戶基本資料與往來記錄管理",           true, 10, 1, now, "System" },
                    { "Suppliers",           "廠商管理",   "供應商基本資料與往來記錄管理",           true, 20, 1, now, "System" },
                    { "Products",            "商品管理",   "商品基本資料、分類與單位管理",           true, 30, 1, now, "System" },
                    { "Purchase",            "採購管理",   "採購訂單、入庫與退貨作業",              true, 40, 1, now, "System" },
                    { "Sales",               "銷售管理",   "報價單、銷貨訂單、出貨與退貨作業",      true, 50, 1, now, "System" },
                    { "Warehouse",           "倉庫管理",   "倉庫、庫存、盤點與領料管理",            true, 60, 1, now, "System" },
                    { "FinancialManagement", "財務管理",   "會計科目、傳票、沖款與收付款管理",      true, 70, 1, now, "System" },
                    { "ProductionManagement","生產管理",   "生產排程、BOM 組成與生產完工入庫",      true, 80, 1, now, "System" },
                    { "Employees",           "員工管理",   "員工資料、部門、職位與權限管理",         true, 90, 1, now, "System" },
                    { "Vehicles",            "車輛管理",   "車輛資料與保養記錄管理",                true, 100, 1, now, "System" },
                    { "WasteManagement",     "廢棄物管理", "廢料類型與廢棄物記錄管理",              true, 110, 1, now, "System" },
                    { "Reports",             "報表",       "各類業務報表列印與匯出",                true, 120, 1, now, "System" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompanyModules");
        }
    }
}
