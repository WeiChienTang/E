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
                new Permission { PermissionCode = "System.Admin", PermissionName = "系統管理員權限", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "User.Create", PermissionName = "建立使用者", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "User.Read", PermissionName = "檢視使用者", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "User.Update", PermissionName = "修改使用者", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "User.Delete", PermissionName = "刪除使用者", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 客戶管理權限
                new Permission { PermissionCode = "Customer.Create", PermissionName = "建立客戶", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Customer.Read", PermissionName = "檢視客戶", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Customer.Update", PermissionName = "修改客戶", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Customer.Delete", PermissionName = "刪除客戶", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },

                // 供應商管理權限
                new Permission { PermissionCode = "Supplier.Create", PermissionName = "建立供應商", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Supplier.Read", PermissionName = "檢視供應商", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Supplier.Update", PermissionName = "修改供應商", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Supplier.Delete", PermissionName = "刪除供應商", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 員工管理權限
                new Permission { PermissionCode = "Employee.Create", PermissionName = "建立員工", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Employee.Read", PermissionName = "檢視員工", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Employee.Update", PermissionName = "修改員工", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Employee.Delete", PermissionName = "刪除員工", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },

                // 部門管理權限
                new Permission { PermissionCode = "Department.Create", PermissionName = "建立部門", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Department.Read", PermissionName = "檢視部門", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Department.Update", PermissionName = "修改部門", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Department.Delete", PermissionName = "刪除部門", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },

                // 產品管理權限
                new Permission { PermissionCode = "Product.Create", PermissionName = "建立產品", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.Read", PermissionName = "檢視產品", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.Update", PermissionName = "修改產品", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.Delete", PermissionName = "刪除產品", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.ViewCost", PermissionName = "檢視成本價", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.ManageStock", PermissionName = "管理庫存", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Product.ManageSupplier", PermissionName = "管理供應商關聯", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },

                // 產品分類管理權限
                new Permission { PermissionCode = "ProductCategory.Create", PermissionName = "建立產品分類", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "ProductCategory.Read", PermissionName = "檢視產品分類", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "ProductCategory.Update", PermissionName = "修改產品分類", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "ProductCategory.Delete", PermissionName = "刪除產品分類", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // MasterData管理權限
                new Permission { PermissionCode = "MasterData.Read", PermissionName = "檢視基礎資料", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Material.Create", PermissionName = "建立材質", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Material.Read", PermissionName = "檢視材質", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Material.Update", PermissionName = "修改材質", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Material.Delete", PermissionName = "刪除材質", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Weather.Create", PermissionName = "建立天氣", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Weather.Read", PermissionName = "檢視天氣", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Weather.Update", PermissionName = "修改天氣", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Weather.Delete", PermissionName = "刪除天氣", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Color.Create", PermissionName = "建立顏色", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Color.Read", PermissionName = "檢視顏色", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Color.Update", PermissionName = "修改顏色", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Color.Delete", PermissionName = "刪除顏色", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Size.Create", PermissionName = "建立尺寸", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Size.Read", PermissionName = "檢視尺寸", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Size.Update", PermissionName = "修改尺寸", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Size.Delete", PermissionName = "刪除尺寸", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Unit.Create", PermissionName = "建立單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Unit.Read", PermissionName = "檢視單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Unit.Update", PermissionName = "修改單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Unit.Delete", PermissionName = "刪除單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SupplierType.Create", PermissionName = "建立供應商類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SupplierType.Read", PermissionName = "檢視供應商類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SupplierType.Update", PermissionName = "修改供應商類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SupplierType.Delete", PermissionName = "刪除供應商類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "CustomerType.Create", PermissionName = "建立客戶類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "CustomerType.Read", PermissionName = "檢視客戶類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "CustomerType.Update", PermissionName = "修改客戶類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "CustomerType.Delete", PermissionName = "刪除客戶類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "EmployeePosition.Create", PermissionName = "建立員工職位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "EmployeePosition.Read", PermissionName = "檢視員工職位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "EmployeePosition.Update", PermissionName = "修改員工職位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "EmployeePosition.Delete", PermissionName = "刪除員工職位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 角色管理權限
                new Permission { PermissionCode = "Role.Create", PermissionName = "建立角色", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Role.Read", PermissionName = "檢視角色", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Role.Update", PermissionName = "修改角色", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Role.Delete", PermissionName = "刪除角色", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 權限管理權限
                new Permission { PermissionCode = "Permission.Create", PermissionName = "建立權限", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Permission.Read", PermissionName = "檢視權限", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Permission.Update", PermissionName = "修改權限", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Permission.Delete", PermissionName = "刪除權限", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 倉庫管理權限
                new Permission { PermissionCode = "Warehouse.Create", PermissionName = "建立倉庫", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Warehouse.Read", PermissionName = "檢視倉庫", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Warehouse.Update", PermissionName = "修改倉庫", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Warehouse.Delete", PermissionName = "刪除倉庫", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存管理權限
                new Permission { PermissionCode = "Inventory.Create", PermissionName = "建立庫存", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Inventory.Read", PermissionName = "檢視庫存", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Inventory.Update", PermissionName = "修改庫存", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Inventory.Delete", PermissionName = "刪除庫存", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Inventory.Overview", PermissionName = "庫存總覽", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Inventory.TransactionHistory", PermissionName = "庫存異動歷史", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存預留權限
                new Permission { PermissionCode = "InventoryReservation.Create", PermissionName = "建立庫存預留", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryReservation.Read", PermissionName = "檢視庫存預留", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryReservation.Update", PermissionName = "修改庫存預留", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryReservation.Delete", PermissionName = "刪除庫存預留", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryReservation.Release", PermissionName = "釋放庫存預留", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryReservation.Cancel", PermissionName = "取消庫存預留", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存盤點權限
                new Permission { PermissionCode = "StockTaking.Create", PermissionName = "建立盤點", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "StockTaking.Read", PermissionName = "檢視盤點", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "StockTaking.Update", PermissionName = "執行盤點", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "StockTaking.Approve", PermissionName = "審核盤點", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "StockTaking.Report", PermissionName = "盤點報告", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存異動權限
                new Permission { PermissionCode = "InventoryTransaction.Create", PermissionName = "建立庫存異動", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryTransaction.Read", PermissionName = "檢視庫存異動", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryTransaction.Update", PermissionName = "修改庫存異動", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryTransaction.Delete", PermissionName = "刪除庫存異動", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存異動類型權限
                new Permission { PermissionCode = "InventoryTransactionType.Create", PermissionName = "建立庫存異動類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryTransactionType.Read", PermissionName = "檢視庫存異動類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryTransactionType.Update", PermissionName = "修改庫存異動類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "InventoryTransactionType.Delete", PermissionName = "刪除庫存異動類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 銷貨管理權限
                new Permission { PermissionCode = "SalesOrder.Create", PermissionName = "建立銷貨訂單", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesOrder.Read", PermissionName = "檢視銷貨訂單", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesOrder.Update", PermissionName = "修改銷貨訂單", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesOrder.Delete", PermissionName = "刪除銷貨訂單", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                new Permission { PermissionCode = "SalesDelivery.Create", PermissionName = "建立銷貨出貨", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesDelivery.Read", PermissionName = "檢視銷貨出貨", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesDelivery.Update", PermissionName = "修改銷貨出貨", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesDelivery.Delete", PermissionName = "刪除銷貨出貨", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                new Permission { PermissionCode = "SalesReturn.Create", PermissionName = "建立銷貨退回", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesReturn.Read", PermissionName = "檢視銷貨退回", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesReturn.Update", PermissionName = "修改銷貨退回", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesReturn.Delete", PermissionName = "刪除銷貨退回", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 採購管理權限
                new Permission { PermissionCode = "Purchase.Read", PermissionName = "檢視採購訂單", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 系統控制權限
                new Permission { PermissionCode = "SystemControl.ViewErrorLog", PermissionName = "檢視錯誤記錄", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SystemControl.ViewUpdates", PermissionName = "檢視更新記錄", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" }
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }
    }
}
