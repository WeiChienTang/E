using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Data.Enums;
using ERPCore2.Data.SeedDataManager.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ERPCore2.Data.SeedDataManager.Seeders
{
    /// <summary>
    /// 權限種子器
    /// </summary>
    public class PermissionSeeder : IDataSeeder
    {
        public int Order => 0;
        public string Name => "權限管理";

        public async Task SeedAsync(AppDbContext context)
        {
            await SeedPermissionsAsync(context);
        }

        /// <summary>
        /// 初始化權限資料
        /// </summary>
        private static async Task SeedPermissionsAsync(AppDbContext context)
        {
            if (await context.Permissions.AnyAsync())
                return;

            var permissions = new[]
            {
                // 系統管理權限
                new Permission { PermissionCode = "System.Admin", PermissionName = "系統管理員權限", Module = "System", Action = "Admin", PermissionGroup = "系統管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "User.Create", PermissionName = "建立使用者", Module = "User", Action = "Create", PermissionGroup = "使用者管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "User.Read", PermissionName = "檢視使用者", Module = "User", Action = "Read", PermissionGroup = "使用者管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "User.Update", PermissionName = "修改使用者", Module = "User", Action = "Update", PermissionGroup = "使用者管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "User.Delete", PermissionName = "刪除使用者", Module = "User", Action = "Delete", PermissionGroup = "使用者管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 客戶管理權限
                new Permission { PermissionCode = "Customer.Create", PermissionName = "建立客戶", Module = "Customer", Action = "Create", PermissionGroup = "客戶管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Customer.Read", PermissionName = "檢視客戶", Module = "Customer", Action = "Read", PermissionGroup = "客戶管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Customer.Update", PermissionName = "修改客戶", Module = "Customer", Action = "Update", PermissionGroup = "客戶管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Customer.Delete", PermissionName = "刪除客戶", Module = "Customer", Action = "Delete", PermissionGroup = "客戶管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },

                // 供應商管理權限
                new Permission { PermissionCode = "Supplier.Create", PermissionName = "建立供應商", Module = "Supplier", Action = "Create", PermissionGroup = "供應商管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Supplier.Read", PermissionName = "檢視供應商", Module = "Supplier", Action = "Read", PermissionGroup = "供應商管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Supplier.Update", PermissionName = "修改供應商", Module = "Supplier", Action = "Update", PermissionGroup = "供應商管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Supplier.Delete", PermissionName = "刪除供應商", Module = "Supplier", Action = "Delete", PermissionGroup = "供應商管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 員工管理權限
                new Permission { PermissionCode = "Employee.Create", PermissionName = "建立員工", Module = "Employee", Action = "Create", PermissionGroup = "員工管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Employee.Read", PermissionName = "檢視員工", Module = "Employee", Action = "Read", PermissionGroup = "員工管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Employee.Update", PermissionName = "修改員工", Module = "Employee", Action = "Update", PermissionGroup = "員工管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Employee.Delete", PermissionName = "刪除員工", Module = "Employee", Action = "Delete", PermissionGroup = "員工管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },

                // 部門管理權限
                new Permission { PermissionCode = "Department.Create", PermissionName = "建立部門", Module = "Department", Action = "Create", PermissionGroup = "員工管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Department.Read", PermissionName = "檢視部門", Module = "Department", Action = "Read", PermissionGroup = "員工管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Department.Update", PermissionName = "修改部門", Module = "Department", Action = "Update", PermissionGroup = "員工管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Department.Delete", PermissionName = "刪除部門", Module = "Department", Action = "Delete", PermissionGroup = "員工管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },

                // 產品管理權限
                new Permission { PermissionCode = "Product.Create", PermissionName = "建立產品", Module = "Product", Action = "Create", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.Read", PermissionName = "檢視產品", Module = "Product", Action = "Read", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.Update", PermissionName = "修改產品", Module = "Product", Action = "Update", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.Delete", PermissionName = "刪除產品", Module = "Product", Action = "Delete", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.ViewCost", PermissionName = "檢視成本價", Module = "Product", Action = "ViewCost", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.ManageStock", PermissionName = "管理庫存", Module = "Product", Action = "ManageStock", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.ManageSupplier", PermissionName = "管理供應商關聯", Module = "Product", Action = "ManageSupplier", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },

                // 產品分類管理權限
                new Permission { PermissionCode = "ProductCategory.Create", PermissionName = "建立產品分類", Module = "ProductCategory", Action = "Create", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "ProductCategory.Read", PermissionName = "檢視產品分類", Module = "ProductCategory", Action = "Read", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "ProductCategory.Update", PermissionName = "修改產品分類", Module = "ProductCategory", Action = "Update", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "ProductCategory.Delete", PermissionName = "刪除產品分類", Module = "ProductCategory", Action = "Delete", PermissionGroup = "產品管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // MasterData管理權限
                new Permission { PermissionCode = "MasterData.Read", PermissionName = "檢視基礎資料", Module = "MasterData", Action = "Read", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Material.Create", PermissionName = "建立材質", Module = "Material", Action = "Create", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Material.Read", PermissionName = "檢視材質", Module = "Material", Action = "Read", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Material.Update", PermissionName = "修改材質", Module = "Material", Action = "Update", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Material.Delete", PermissionName = "刪除材質", Module = "Material", Action = "Delete", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Weather.Create", PermissionName = "建立天氣", Module = "Weather", Action = "Create", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Weather.Read", PermissionName = "檢視天氣", Module = "Weather", Action = "Read", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Weather.Update", PermissionName = "修改天氣", Module = "Weather", Action = "Update", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Weather.Delete", PermissionName = "刪除天氣", Module = "Weather", Action = "Delete", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Color.Create", PermissionName = "建立顏色", Module = "Color", Action = "Create", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Color.Read", PermissionName = "檢視顏色", Module = "Color", Action = "Read", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Color.Update", PermissionName = "修改顏色", Module = "Color", Action = "Update", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Color.Delete", PermissionName = "刪除顏色", Module = "Color", Action = "Delete", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Size.Create", PermissionName = "建立尺寸", Module = "Size", Action = "Create", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Size.Read", PermissionName = "檢視尺寸", Module = "Size", Action = "Read", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Size.Update", PermissionName = "修改尺寸", Module = "Size", Action = "Update", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Size.Delete", PermissionName = "刪除尺寸", Module = "Size", Action = "Delete", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Unit.Create", PermissionName = "建立單位", Module = "Unit", Action = "Create", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Unit.Read", PermissionName = "檢視單位", Module = "Unit", Action = "Read", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Unit.Update", PermissionName = "修改單位", Module = "Unit", Action = "Update", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Unit.Delete", PermissionName = "刪除單位", Module = "Unit", Action = "Delete", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SupplierType.Create", PermissionName = "建立供應商類型", Module = "SupplierType", Action = "Create", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SupplierType.Read", PermissionName = "檢視供應商類型", Module = "SupplierType", Action = "Read", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SupplierType.Update", PermissionName = "修改供應商類型", Module = "SupplierType", Action = "Update", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SupplierType.Delete", PermissionName = "刪除供應商類型", Module = "SupplierType", Action = "Delete", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "CustomerType.Create", PermissionName = "建立客戶類型", Module = "CustomerType", Action = "Create", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "CustomerType.Read", PermissionName = "檢視客戶類型", Module = "CustomerType", Action = "Read", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "CustomerType.Update", PermissionName = "修改客戶類型", Module = "CustomerType", Action = "Update", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "CustomerType.Delete", PermissionName = "刪除客戶類型", Module = "CustomerType", Action = "Delete", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "EmployeePosition.Create", PermissionName = "建立員工職位", Module = "EmployeePosition", Action = "Create", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "EmployeePosition.Read", PermissionName = "檢視員工職位", Module = "EmployeePosition", Action = "Read", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "EmployeePosition.Update", PermissionName = "修改員工職位", Module = "EmployeePosition", Action = "Update", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "EmployeePosition.Delete", PermissionName = "刪除員工職位", Module = "EmployeePosition", Action = "Delete", PermissionGroup = "基礎單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 角色管理權限
                new Permission { PermissionCode = "Role.Create", PermissionName = "建立角色", Module = "Role", Action = "Create", PermissionGroup = "權限管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Role.Read", PermissionName = "檢視角色", Module = "Role", Action = "Read", PermissionGroup = "權限管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Role.Update", PermissionName = "修改角色", Module = "Role", Action = "Update", PermissionGroup = "權限管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Role.Delete", PermissionName = "刪除角色", Module = "Role", Action = "Delete", PermissionGroup = "權限管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 權限管理權限
                new Permission { PermissionCode = "Permission.Create", PermissionName = "建立權限", Module = "Permission", Action = "Create", PermissionGroup = "權限管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Permission.Read", PermissionName = "檢視權限", Module = "Permission", Action = "Read", PermissionGroup = "權限管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Permission.Update", PermissionName = "修改權限", Module = "Permission", Action = "Update", PermissionGroup = "權限管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Permission.Delete", PermissionName = "刪除權限", Module = "Permission", Action = "Delete", PermissionGroup = "權限管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 倉庫管理權限
                new Permission { PermissionCode = "Warehouse.Create", PermissionName = "建立倉庫", Module = "Warehouse", Action = "Create", PermissionGroup = "倉庫管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Warehouse.Read", PermissionName = "檢視倉庫", Module = "Warehouse", Action = "Read", PermissionGroup = "倉庫管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Warehouse.Update", PermissionName = "修改倉庫", Module = "Warehouse", Action = "Update", PermissionGroup = "倉庫管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Warehouse.Delete", PermissionName = "刪除倉庫", Module = "Warehouse", Action = "Delete", PermissionGroup = "倉庫管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存管理權限
                new Permission { PermissionCode = "Inventory.Create", PermissionName = "建立庫存", Module = "Inventory", Action = "Create", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Inventory.Read", PermissionName = "檢視庫存", Module = "Inventory", Action = "Read", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Inventory.Update", PermissionName = "修改庫存", Module = "Inventory", Action = "Update", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Inventory.Delete", PermissionName = "刪除庫存", Module = "Inventory", Action = "Delete", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Inventory.Overview", PermissionName = "庫存總覽", Module = "Inventory", Action = "Overview", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Inventory.TransactionHistory", PermissionName = "庫存異動歷史", Module = "Inventory", Action = "TransactionHistory", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存預留權限
                new Permission { PermissionCode = "InventoryReservation.Create", PermissionName = "建立庫存預留", Module = "InventoryReservation", Action = "Create", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryReservation.Read", PermissionName = "檢視庫存預留", Module = "InventoryReservation", Action = "Read", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryReservation.Update", PermissionName = "修改庫存預留", Module = "InventoryReservation", Action = "Update", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryReservation.Delete", PermissionName = "刪除庫存預留", Module = "InventoryReservation", Action = "Delete", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryReservation.Release", PermissionName = "釋放庫存預留", Module = "InventoryReservation", Action = "Release", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryReservation.Cancel", PermissionName = "取消庫存預留", Module = "InventoryReservation", Action = "Cancel", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存盤點權限
                new Permission { PermissionCode = "StockTaking.Create", PermissionName = "建立盤點", Module = "StockTaking", Action = "Create", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "StockTaking.Read", PermissionName = "檢視盤點", Module = "StockTaking", Action = "Read", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "StockTaking.Update", PermissionName = "執行盤點", Module = "StockTaking", Action = "Update", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "StockTaking.Approve", PermissionName = "審核盤點", Module = "StockTaking", Action = "Approve", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "StockTaking.Report", PermissionName = "盤點報告", Module = "StockTaking", Action = "Report", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存異動權限
                new Permission { PermissionCode = "InventoryTransaction.Create", PermissionName = "建立庫存異動", Module = "InventoryTransaction", Action = "Create", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryTransaction.Read", PermissionName = "檢視庫存異動", Module = "InventoryTransaction", Action = "Read", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryTransaction.Update", PermissionName = "修改庫存異動", Module = "InventoryTransaction", Action = "Update", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryTransaction.Delete", PermissionName = "刪除庫存異動", Module = "InventoryTransaction", Action = "Delete", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存異動類型權限
                new Permission { PermissionCode = "InventoryTransactionType.Create", PermissionName = "建立庫存異動類型", Module = "InventoryTransactionType", Action = "Create", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryTransactionType.Read", PermissionName = "檢視庫存異動類型", Module = "InventoryTransactionType", Action = "Read", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryTransactionType.Update", PermissionName = "修改庫存異動類型", Module = "InventoryTransactionType", Action = "Update", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryTransactionType.Delete", PermissionName = "刪除庫存異動類型", Module = "InventoryTransactionType", Action = "Delete", PermissionGroup = "庫存管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 銷貨管理權限
                new Permission { PermissionCode = "SalesOrder.Create", PermissionName = "建立銷貨訂單", Module = "SalesOrder", Action = "Create", PermissionGroup = "銷貨管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesOrder.Read", PermissionName = "檢視銷貨訂單", Module = "SalesOrder", Action = "Read", PermissionGroup = "銷貨管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesOrder.Update", PermissionName = "修改銷貨訂單", Module = "SalesOrder", Action = "Update", PermissionGroup = "銷貨管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesOrder.Delete", PermissionName = "刪除銷貨訂單", Module = "SalesOrder", Action = "Delete", PermissionGroup = "銷貨管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                new Permission { PermissionCode = "SalesDelivery.Create", PermissionName = "建立銷貨出貨", Module = "SalesDelivery", Action = "Create", PermissionGroup = "銷貨管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesDelivery.Read", PermissionName = "檢視銷貨出貨", Module = "SalesDelivery", Action = "Read", PermissionGroup = "銷貨管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesDelivery.Update", PermissionName = "修改銷貨出貨", Module = "SalesDelivery", Action = "Update", PermissionGroup = "銷貨管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesDelivery.Delete", PermissionName = "刪除銷貨出貨", Module = "SalesDelivery", Action = "Delete", PermissionGroup = "銷貨管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                new Permission { PermissionCode = "SalesReturn.Create", PermissionName = "建立銷貨退回", Module = "SalesReturn", Action = "Create", PermissionGroup = "銷貨管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesReturn.Read", PermissionName = "檢視銷貨退回", Module = "SalesReturn", Action = "Read", PermissionGroup = "銷貨管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesReturn.Update", PermissionName = "修改銷貨退回", Module = "SalesReturn", Action = "Update", PermissionGroup = "銷貨管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesReturn.Delete", PermissionName = "刪除銷貨退回", Module = "SalesReturn", Action = "Delete", PermissionGroup = "銷貨管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 採購管理權限
                new Permission { PermissionCode = "Purchase.Read", PermissionName = "檢視採購訂單", Module = "Purchase", Action = "Read", PermissionGroup = "採購管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 系統控制權限
                new Permission { PermissionCode = "SystemControl.ViewErrorLog", PermissionName = "檢視錯誤記錄", Module = "SystemControl", Action = "ViewErrorLog", PermissionGroup = "系統管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SystemControl.ViewUpdates", PermissionName = "檢視更新記錄", Module = "SystemControl", Action = "ViewUpdates", PermissionGroup = "系統管理", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" }
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }
    }
}
