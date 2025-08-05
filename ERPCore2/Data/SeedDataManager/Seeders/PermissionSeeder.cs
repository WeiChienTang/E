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
                // 使用者管理權限
                new Permission { PermissionCode = "User.Read", PermissionName = "檢視使用者", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 客戶管理權限
                new Permission { PermissionCode = "Customer.Read", PermissionName = "檢視客戶", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },

                // 供應商管理權限
                new Permission { PermissionCode = "Supplier.Read", PermissionName = "檢視供應商", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 員工管理權限
                new Permission { PermissionCode = "Employee.Read", PermissionName = "檢視員工", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },

                // 部門管理權限
                new Permission { PermissionCode = "Department.Read", PermissionName = "檢視部門", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },

                // 產品管理權限
                new Permission { PermissionCode = "Product.Read", PermissionName = "檢視產品", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },

                // 產品分類管理權限
                new Permission { PermissionCode = "ProductCategory.Read", PermissionName = "檢視產品分類", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // MasterData管理權限
                new Permission { PermissionCode = "MasterData.Read", PermissionName = "檢視基礎資料", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Material.Read", PermissionName = "檢視材質", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Weather.Read", PermissionName = "檢視天氣", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Color.Read", PermissionName = "檢視顏色", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Size.Read", PermissionName = "檢視尺寸", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "Unit.Read", PermissionName = "檢視單位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SupplierType.Read", PermissionName = "檢視供應商類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "CustomerType.Read", PermissionName = "檢視客戶類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "EmployeePosition.Read", PermissionName = "檢視員工職位", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 角色管理權限
                new Permission { PermissionCode = "Role.Read", PermissionName = "檢視角色", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 權限管理權限
                new Permission { PermissionCode = "Permission.Read", PermissionName = "檢視權限", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 倉庫管理權限
                new Permission { PermissionCode = "Warehouse.Read", PermissionName = "檢視倉庫", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存管理權限
                new Permission { PermissionCode = "Inventory.Read", PermissionName = "檢視庫存", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存預留權限
                new Permission { PermissionCode = "InventoryReservation.Read", PermissionName = "檢視庫存預留", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存盤點權限
                new Permission { PermissionCode = "StockTaking.Read", PermissionName = "檢視盤點", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存異動權限
                new Permission { PermissionCode = "InventoryTransaction.Read", PermissionName = "檢視庫存異動", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 庫存異動類型權限
                new Permission { PermissionCode = "InventoryTransactionType.Read", PermissionName = "檢視庫存異動類型", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 銷貨管理權限
                new Permission { PermissionCode = "SalesOrder.Read", PermissionName = "檢視銷貨訂單", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesDelivery.Read", PermissionName = "檢視銷貨出貨", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                new Permission { PermissionCode = "SalesReturn.Read", PermissionName = "檢視銷貨退回", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" },
                
                // 採購管理權限
                new Permission { PermissionCode = "Purchase.Read", PermissionName = "檢視採購訂單", Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System" }
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }
    }
}
