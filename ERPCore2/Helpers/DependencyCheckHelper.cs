using ERPCore2.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERPCore2.Helpers
{
    /// <summary>
    /// 關聯依賴檢查輔助類別
    /// 用於檢查實體是否可以被刪除（檢查外鍵關聯）
    /// </summary>
    public static class DependencyCheckHelper
    {
        /// <summary>
        /// 檢查實體的依賴關係結果
        /// </summary>
        public class DependencyCheckResult
        {
            public bool CanDelete { get; set; }
            public List<string> DependentEntities { get; set; } = new();
            public string ErrorMessage { get; set; } = string.Empty;
            
            /// <summary>
            /// 取得格式化的錯誤訊息
            /// </summary>
            public string GetFormattedErrorMessage(string entityName)
            {
                if (CanDelete)
                    return string.Empty;
                    
                if (DependentEntities.Any())
                {
                    var dependentList = string.Join("、", DependentEntities);
                    return $"無法刪除此{entityName}，因為有以下資料正在使用：{dependentList}";
                }
                
                return !string.IsNullOrEmpty(ErrorMessage) ? ErrorMessage : $"無法刪除此{entityName}，因為有其他資料正在使用";
            }
        }
        
        /// <summary>
        /// 檢查部門是否可以刪除
        /// </summary>
        public static async Task<DependencyCheckResult> CheckDepartmentDependenciesAsync(IDbContextFactory<AppDbContext> contextFactory, int departmentId)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var result = new DependencyCheckResult { CanDelete = true };
                
                // 檢查員工
                var employeeCount = await context.Employees
                    .CountAsync(e => e.DepartmentId == departmentId);
                    
                if (employeeCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"員工({employeeCount}筆)");
                }
                
                return result;
            }
            catch (Exception)
            {
                return new DependencyCheckResult 
                { 
                    CanDelete = false, 
                    ErrorMessage = "檢查部門依賴關係時發生錯誤" 
                };
            }
        }
        
        /// <summary>
        /// 檢查角色是否可以刪除
        /// </summary>
        public static async Task<DependencyCheckResult> CheckRoleDependenciesAsync(IDbContextFactory<AppDbContext> contextFactory, int roleId)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var result = new DependencyCheckResult { CanDelete = true };
                
                // 檢查員工
                var employeeCount = await context.Employees
                    .CountAsync(e => e.RoleId == roleId);
                    
                if (employeeCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"員工({employeeCount}筆)");
                }
                
                return result;
            }
            catch (Exception)
            {
                return new DependencyCheckResult 
                { 
                    CanDelete = false, 
                    ErrorMessage = "檢查角色依賴關係時發生錯誤" 
                };
            }
        }
        
        /// <summary>
        /// 檢查職位是否可以刪除
        /// </summary>
        public static async Task<DependencyCheckResult> CheckEmployeePositionDependenciesAsync(IDbContextFactory<AppDbContext> contextFactory, int positionId)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var result = new DependencyCheckResult { CanDelete = true };
                
                // 檢查員工
                var employeeCount = await context.Employees
                    .CountAsync(e => e.EmployeePositionId == positionId);
                    
                if (employeeCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"員工({employeeCount}筆)");
                }
                
                return result;
            }
            catch (Exception)
            {
                return new DependencyCheckResult 
                { 
                    CanDelete = false, 
                    ErrorMessage = "檢查職位依賴關係時發生錯誤" 
                };
            }
        }
        
        /// <summary>
        /// 檢查供應商是否可以刪除
        /// </summary>
        public static async Task<DependencyCheckResult> CheckSupplierDependenciesAsync(IDbContextFactory<AppDbContext> contextFactory, int supplierId)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var result = new DependencyCheckResult { CanDelete = true };
                
                // 檢查採購訂單
                var purchaseOrderCount = await context.PurchaseOrders
                    .CountAsync(po => po.SupplierId == supplierId);
                    
                if (purchaseOrderCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"採購訂單({purchaseOrderCount}筆)");
                }
                
                // 檢查商品供應商關聯
                var productSupplierCount = await context.ProductSuppliers
                    .CountAsync(ps => ps.SupplierId == supplierId);
                    
                if (productSupplierCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"商品供應商關聯({productSupplierCount}筆)");
                }
                
                return result;
            }
            catch (Exception)
            {
                return new DependencyCheckResult 
                { 
                    CanDelete = false, 
                    ErrorMessage = "檢查供應商依賴關係時發生錯誤" 
                };
            }
        }
        
        /// <summary>
        /// 檢查客戶是否可以刪除
        /// </summary>
        public static async Task<DependencyCheckResult> CheckCustomerDependenciesAsync(IDbContextFactory<AppDbContext> contextFactory, int customerId)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var result = new DependencyCheckResult { CanDelete = true };
                
                // 檢查銷貨訂單
                var salesOrderCount = await context.SalesOrders
                    .CountAsync(so => so.CustomerId == customerId);
                    
                if (salesOrderCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"銷貨訂單({salesOrderCount}筆)");
                }
                
                return result;
            }
            catch (Exception)
            {
                return new DependencyCheckResult 
                { 
                    CanDelete = false, 
                    ErrorMessage = "檢查客戶依賴關係時發生錯誤" 
                };
            }
        }
        
        /// <summary>
        /// 檢查商品是否可以刪除
        /// </summary>
        public static async Task<DependencyCheckResult> CheckProductDependenciesAsync(IDbContextFactory<AppDbContext> contextFactory, int productId)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var result = new DependencyCheckResult { CanDelete = true };
                
                // 檢查採購訂單明細
                var purchaseOrderDetailCount = await context.PurchaseOrderDetails
                    .CountAsync(pod => pod.ProductId == productId);
                    
                if (purchaseOrderDetailCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"採購訂單明細({purchaseOrderDetailCount}筆)");
                }
                
                // 檢查銷貨訂單明細
                var salesOrderDetailCount = await context.SalesOrderDetails
                    .CountAsync(sod => sod.ProductId == productId);
                    
                if (salesOrderDetailCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"銷貨訂單明細({salesOrderDetailCount}筆)");
                }
                
                // 檢查庫存
                var inventoryCount = await context.InventoryStocks
                    .CountAsync(i => i.ProductId == productId);
                    
                if (inventoryCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"庫存記錄({inventoryCount}筆)");
                }
                
                // 檢查商品供應商關聯
                var productSupplierCount = await context.ProductSuppliers
                    .CountAsync(ps => ps.ProductId == productId);
                    
                if (productSupplierCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"商品供應商關聯({productSupplierCount}筆)");
                }
                
                return result;
            }
            catch (Exception)
            {
                return new DependencyCheckResult 
                { 
                    CanDelete = false, 
                    ErrorMessage = "檢查商品依賴關係時發生錯誤" 
                };
            }
        }
        
        /// <summary>
        /// 檢查倉庫是否可以刪除
        /// </summary>
        public static async Task<DependencyCheckResult> CheckWarehouseDependenciesAsync(IDbContextFactory<AppDbContext> contextFactory, int warehouseId)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var result = new DependencyCheckResult { CanDelete = true };
                
                // 檢查庫存
                var inventoryCount = await context.InventoryStockDetails
                    .CountAsync(i => i.WarehouseId == warehouseId);
                    
                if (inventoryCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"庫存記錄({inventoryCount}筆)");
                }

                // 檢查進貨記錄明細
                var purchaseReceivingDetailCount = await context.PurchaseReceivingDetails
                    .CountAsync(prd => prd.WarehouseId == warehouseId);
                    
                if (purchaseReceivingDetailCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"進貨記錄明細({purchaseReceivingDetailCount}筆)");
                }

                // 檢查採購訂單
                var purchaseOrderCount = await context.PurchaseOrders
                    .CountAsync(po => po.WarehouseId == warehouseId);
                    
                if (purchaseOrderCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"採購訂單({purchaseOrderCount}筆)");
                }



                // 檢查庫存異動記錄
                var inventoryTransactionCount = await context.InventoryTransactions
                    .CountAsync(it => it.WarehouseId == warehouseId);
                    
                if (inventoryTransactionCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"庫存異動記錄({inventoryTransactionCount}筆)");
                }

                // 檢查盤點記錄
                var stockTakingCount = await context.StockTakings
                    .CountAsync(st => st.WarehouseId == warehouseId);
                    
                if (stockTakingCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"盤點記錄({stockTakingCount}筆)");
                }

                // 檢查倉庫位置
                var warehouseLocationCount = await context.WarehouseLocations
                    .CountAsync(wl => wl.WarehouseId == warehouseId);
                    
                if (warehouseLocationCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"倉庫位置({warehouseLocationCount}筆)");
                }

                // 檢查庫存預留
                var inventoryReservationCount = await context.InventoryReservations
                    .CountAsync(ir => ir.WarehouseId == warehouseId);
                    
                if (inventoryReservationCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"庫存預留({inventoryReservationCount}筆)");
                }
                
                return result;
            }
            catch (Exception)
            {
                return new DependencyCheckResult 
                { 
                    CanDelete = false, 
                    ErrorMessage = "檢查倉庫依賴關係時發生錯誤" 
                };
            }
        }
        
        /// <summary>
        /// 檢查單位是否可以刪除
        /// </summary>
        public static async Task<DependencyCheckResult> CheckUnitDependenciesAsync(IDbContextFactory<AppDbContext> contextFactory, int unitId)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var result = new DependencyCheckResult { CanDelete = true };
                
                // 檢查商品
                var productCount = await context.Products
                    .CountAsync(p => p.UnitId == unitId);
                    
                if (productCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"商品({productCount}筆)");
                }
                
                // 檢查採購訂單明細
                var purchaseOrderDetailCount = await context.PurchaseOrderDetails
                    .Join(context.Products.Where(p => p.UnitId == unitId),
                          pod => pod.ProductId,
                          p => p.Id,
                          (pod, p) => pod)
                    .CountAsync();
                    
                if (purchaseOrderDetailCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"採購訂單明細({purchaseOrderDetailCount}筆)");
                }
                
                // 檢查銷貨訂單明細
                var salesOrderDetailCount = await context.SalesOrderDetails
                    .CountAsync(sod => sod.UnitId == unitId);
                    
                if (salesOrderDetailCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"銷貨訂單明細({salesOrderDetailCount}筆)");
                }
                
                return result;
            }
            catch (Exception)
            {
                return new DependencyCheckResult 
                { 
                    CanDelete = false, 
                    ErrorMessage = "檢查單位依賴關係時發生錯誤" 
                };
            }
        }

        /// <summary>
        /// 檢查採購單是否可以刪除
        /// </summary>
        public static async Task<DependencyCheckResult> CheckPurchaseOrderDependenciesAsync(IDbContextFactory<AppDbContext> contextFactory, int purchaseOrderId)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var result = new DependencyCheckResult { CanDelete = true };

                // 檢查是否有進貨明細記錄 (PurchaseReceivingDetails)
                var receivingDetailCount = await context.PurchaseReceivingDetails
                    .CountAsync(prd => context.PurchaseOrderDetails
                        .Where(pod => pod.PurchaseOrderId == purchaseOrderId)
                        .Select(pod => pod.Id)
                        .Contains(prd.PurchaseOrderDetailId));

                if (receivingDetailCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"進貨明細記錄({receivingDetailCount}筆)");
                }

                return result;
            }
            catch (Exception)
            {
                return new DependencyCheckResult 
                { 
                    CanDelete = false, 
                    ErrorMessage = "檢查採購單依賴關係時發生錯誤" 
                };
            }
        }

        /// <summary>
        /// 檢查權限是否可以刪除
        /// </summary>
        public static async Task<DependencyCheckResult> CheckPermissionDependenciesAsync(IDbContextFactory<AppDbContext> contextFactory, int permissionId)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var result = new DependencyCheckResult { CanDelete = true };

                // 檢查角色權限關聯
                var rolePermissionCount = await context.RolePermissions
                    .CountAsync(rp => rp.PermissionId == permissionId);

                if (rolePermissionCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"角色權限關聯({rolePermissionCount}筆)");
                }

                return result;
            }
            catch (Exception)
            {
                return new DependencyCheckResult 
                { 
                    CanDelete = false, 
                    ErrorMessage = "檢查權限依賴關係時發生錯誤" 
                };
            }
        }

        /// <summary>
        /// 檢查尺寸是否可以刪除
        /// </summary>
        public static async Task<DependencyCheckResult> CheckSizeDependenciesAsync(IDbContextFactory<AppDbContext> contextFactory, int sizeId)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var result = new DependencyCheckResult { CanDelete = true };

                // 檢查商品
                var productCount = await context.Products
                    .CountAsync(p => p.SizeId == sizeId);

                if (productCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"商品({productCount}筆)");
                }

                return result;
            }
            catch (Exception)
            {
                return new DependencyCheckResult 
                { 
                    CanDelete = false, 
                    ErrorMessage = "檢查尺寸依賴關係時發生錯誤" 
                };
            }
        }

        /// <summary>
        /// 檢查公司是否可以刪除
        /// </summary>
        public static async Task<DependencyCheckResult> CheckCompanyDependenciesAsync(IDbContextFactory<AppDbContext> contextFactory, int companyId)
        {
            try
            {
                using var context = await contextFactory.CreateDbContextAsync();
                var result = new DependencyCheckResult { CanDelete = true };

                // 檢查採購訂單
                var purchaseOrderCount = await context.PurchaseOrders
                    .CountAsync(po => po.CompanyId == companyId);

                if (purchaseOrderCount > 0)
                {
                    result.CanDelete = false;
                    result.DependentEntities.Add($"採購訂單({purchaseOrderCount}筆)");
                }

                return result;
            }
            catch (Exception)
            {
                return new DependencyCheckResult 
                { 
                    CanDelete = false, 
                    ErrorMessage = "檢查公司依賴關係時發生錯誤" 
                };
            }
        }
    }
}
