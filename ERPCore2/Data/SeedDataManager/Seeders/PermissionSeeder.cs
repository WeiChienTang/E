using ERPCore2.Data.Context;
using ERPCore2.Data.Entities;
using ERPCore2.Models.Enums;
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
                // ========== 敏感權限 (Sensitive) - 需要更高權限者授予 ==========
                
                // 系統管理權限 (請勿刪除，此為最高權限)
                new Permission { Code = "System.Admin", Name = "系統管理", Level = PermissionLevel.Sensitive, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "系統最高管理權限，擁有所有功能存取權限" },
                new Permission { Code = "EmployeeEdit_Account_Password.Read", Name = "編輯員工帳號密碼", Level = PermissionLevel.Sensitive, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "編輯員工系統帳號、密碼與角色設定權限" },
                new Permission { Code = "Permission.Read", Name = "檢視權限", Level = PermissionLevel.Sensitive, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視系統功能權限設定" },
                new Permission { Code = "Role.Read", Name = "檢視角色", Level = PermissionLevel.Sensitive, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視系統角色與權限群組設定" },
                new Permission { Code = "Company.Read", Name = "檢視公司", Level = PermissionLevel.Sensitive, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視公司基本資料與相關資訊"},
                new Permission { Code = "SystemControl.Read", Name = "檢視系統控制", Level = PermissionLevel.Sensitive, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視系統設定與控制功能" },
                new Permission { Code = "PurchaseOrder.Approve", Name = "審核採購訂單", Level = PermissionLevel.Sensitive, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "審核與核准採購訂單權限" },
                new Permission { Code = "Quotation.Approve", Name = "審核報價單", Level = PermissionLevel.Sensitive, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "審核與核准報價單" },
                
                // ========== 一般權限 (Normal) - 可授予一般員工 ==========
                
                // 系統設定相關
                new Permission { Code = "PaperSetting.Read", Name = "檢視紙張設定", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視紙張設定基本資料與相關資訊" },
                new Permission { Code = "PrinterSetting.Read", Name = "檢視印表機設定", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視印表機設定基本資料與相關資訊" },
                new Permission { Code = "ReportPrintConfiguration.Read", Name = "檢視報表列印設定", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視報表列印設定基本資料與相關資訊" },
                new Permission { Code = "SystemParameter.Read", Name = "檢視系統參數", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視系統全域參數設定" },

                // 使用者管理權限
                new Permission { Code = "User.Read", Name = "檢視使用者", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視系統使用者基本資料" },
                
                // 客戶管理權限
                new Permission { Code = "Customer.Read", Name = "檢視客戶", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視客戶基本資料與相關資訊" },

                // 供應商管理權限
                new Permission { Code = "Supplier.Read", Name = "檢視供應商", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視供應商基本資料與相關資訊" },
                
                // 員工相關管理權限
                new Permission { Code = "Employee.Read", Name = "檢視員工", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視員工基本資料與組織架構" },
                new Permission { Code = "Department.Read", Name = "檢視部門", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視公司部門組織架構資料" },
                new Permission { Code = "EmployeePosition.Read", Name = "檢視員工職位", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視員工職位與職級設定" },

                // 商品管理權限
                new Permission { Code = "Product.Read", Name = "檢視商品", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視商品基本資料與規格" },
                new Permission { Code = "ProductCategory.Read", Name = "檢視商品分類", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視商品分類階層與設定" },
                new Permission { Code = "ProductComposition.Read", Name = "檢視商品合成", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視商品合成（BOM）結構與明細" },
                new Permission { Code = "CompositionCategory.Read", Name = "檢視物料清單類型", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視商品物料清單的類型分類" },
                new Permission { Code = "ProductionSchedule.Read", Name = "檢視生產排程", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視生產排程的詳細資料" },

                // 商品定價管理權限
                new Permission { Code = "ProductPricing.Read", Name = "檢視商品定價", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視商品價格設定與價格表" },
                new Permission { Code = "MasterData.Read", Name = "檢視基礎資料", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視系統基礎資料維護功能" },
                new Permission { Code = "Material.Read", Name = "檢視材質", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視商品材質分類與屬性" },
                new Permission { Code = "Weather.Read", Name = "檢視天氣", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視天氣相關基礎資料" },
                new Permission { Code = "Color.Read", Name = "檢視顏色", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視顏色分類與色彩設定" },
                new Permission { Code = "Size.Read", Name = "檢視尺寸", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視尺寸規格與大小設定" },
                new Permission { Code = "Unit.Read", Name = "檢視單位", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視度量衡單位與換算設定" },
                new Permission { Code = "CustomerType.Read", Name = "檢視客戶類型", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視客戶分類與類型設定" },
                
                // 車輛管理權限
                new Permission { Code = "Vehicle.Read", Name = "檢視車輛", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視車輛基本資料與相關資訊" },
                new Permission { Code = "VehicleType.Read", Name = "檢視車輛類型", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視車輛類型基本資料與相關資訊" },
                
                // 倉庫管理權限
                new Permission { Code = "Warehouse.Read", Name = "檢視倉庫", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視倉庫基本資料與儲位設定" },
                new Permission { Code = "Inventory.Read", Name = "檢視庫存", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視庫存數量與庫存狀況" },
                new Permission { Code = "WarehouseLocation.Read", Name = "檢視倉庫位置", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視倉庫內部位置與儲位設定" },
                new Permission { Code = "InventoryTransaction.Read", Name = "檢視庫存異動", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視庫存進出異動記錄" },
                new Permission { Code = "MaterialIssue.Read", Name = "檢視領料單", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視領料單基本資料與明細" },
                
                // 庫存明細權限
                new Permission { Code = "InventoryStock.Read", Name = "檢視庫存明細", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視詳細庫存明細與批號資訊" },
                
                // 庫存預留權限
                new Permission { Code = "InventoryReservation.Read", Name = "檢視庫存預留", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視庫存預留與保留狀況" },
                
                // 庫存盤點權限
                new Permission { Code = "StockTaking.Read", Name = "檢視盤點", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視庫存盤點作業與結果" },

                // 財務管理權限
                new Permission { Code = "SetoffDocument.Read", Name = "檢視沖款單", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視客戶應收帳款與交易紀錄" },
                new Permission { Code = "PaymentMethod.Read", Name = "檢視付款方式", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視系統付款方式設定" },
                new Permission { Code = "FinancialTransaction.Read", Name = "檢視財務交易", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視財務交易紀錄與明細" },
                new Permission { Code = "Bank.Read", Name = "檢視銀行", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視銀行基本資料與相關資訊"},
                new Permission { Code = "Currency.Read", Name = "檢視貨幣", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視貨幣基本資料與匯率設定"},

                // 庫存異動類型權限
                new Permission { Code = "InventoryTransactionType.Read", Name = "檢視庫存異動類型", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視庫存異動類型與分類設定" },
                
                // 銷貨管理權限
                new Permission { Code = "Quotation.Read", Name = "檢視報價單", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視報價單與客戶交易紀錄" },
                new Permission { Code = "Sales.Read", Name = "檢視銷售訂單", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視銷售訂單與客戶交易紀錄" },
                new Permission { Code = "SalesOrder.Read", Name = "檢視銷貨訂單", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視銷貨訂單詳細資料" },
                new Permission { Code = "SalesDelivery.Read", Name = "檢視銷貨出貨", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視銷貨出貨單與配送記錄" },
                new Permission { Code = "SalesReturn.Read", Name = "檢視銷貨退回", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視銷貨退回與退貨處理" },
                new Permission { Code = "SalesReturnReason.Read", Name = "檢視銷貨退回原因", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視銷貨退回原因設定與管理" },

                // 採購管理權限
                new Permission { Code = "PurchaseOrder.Read", Name = "檢視採購訂單", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視採購訂單與供應商交易" },
                new Permission { Code = "PurchaseReceiving.Read", Name = "檢視採購收貨", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視採購收貨單與驗收記錄" },
                new Permission { Code = "PurchaseReturn.Read", Name = "檢視採購退回貨", Level = PermissionLevel.Normal, Status = EntityStatus.Active, CreatedAt = DateTime.Now, CreatedBy = "System", Remarks = "檢視採購退貨與退回處理" }
            };

            await context.Permissions.AddRangeAsync(permissions);
            await context.SaveChangesAsync();
        }
    }
}
