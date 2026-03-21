using ERPCore2.Models.Enums;

namespace ERPCore2.Models;

/// <summary>
/// 權限定義資料傳輸物件，供 PermissionSeeder 使用
/// GroupKey 為導航分組 resource key（如 "Nav.AccountingGroup"），供權限管理 UI 分類使用
/// </summary>
public record PermissionDefinition(string Code, string Name, PermissionLevel Level, string Remarks, string GroupKey = "Nav.SystemGroup");

/// <summary>
/// 權限登錄表 - 系統所有權限的單一來源
/// 新增權限時只需在此處加入，Seeder 會自動同步至資料庫
/// </summary>
public static class PermissionRegistry
{
    // ========== 敏感權限 (Sensitive) ==========

    public static class System
    {
        public const string Admin = "System.Admin";
    }

    public static class EmployeeAccount
    {
        public const string Read = "EmployeeEdit_Account_Password.Read";
    }

    public static class Permission
    {
        public const string Read = "Permission.Read";
    }

    public static class Role
    {
        public const string Read = "Role.Read";
    }

    public static class Company
    {
        public const string Read = "Company.Read";
    }

    public static class SystemControl
    {
        public const string Read = "SystemControl.Read";
    }

    public static class Document
    {
        public const string Read = "Document.Read";
        public const string Sensitive = "Document.Sensitive";
        public const string Manage = "Document.Manage";
    }

    // ========== 一般權限 (Normal) ==========

    public static class PaperSetting
    {
        public const string Read = "PaperSetting.Read";
    }

    public static class ReportPrintConfiguration
    {
        public const string Read = "ReportPrintConfiguration.Read";
    }

    public static class SystemParameter
    {
        public const string Read = "SystemParameter.Read";
    }

    public static class ErrorLog
    {
        public const string Read = "ErrorLog.Read";
    }

    public static class User
    {
        public const string Read = "User.Read";
    }

    public static class Employee
    {
        public const string Read      = "Employee.Read";
        public const string ChartRead = "EmployeeChart.Read";
    }

    public static class Department
    {
        public const string Read = "Department.Read";
    }

    public static class EmployeePosition
    {
        public const string Read = "EmployeePosition.Read";
    }

    public static class Customer
    {
        public const string Read = "Customer.Read";
        public const string ChartRead = "CustomerChart.Read";
    }

    public static class CrmLead
    {
        public const string Read = "CrmLead.Read";
    }

    public static class CustomerComplaint
    {
        public const string Read = "CustomerComplaint.Read";
    }

    public static class CustomerType
    {
        public const string Read = "CustomerType.Read";
    }

    public static class Supplier
    {
        public const string Read = "Supplier.Read";
        public const string ChartRead = "SupplierChart.Read";
    }

    public static class Item
    {
        public const string Read      = "Item.Read";
        public const string ChartRead = "ItemChart.Read";
    }

    public static class ItemCategory
    {
        public const string Read = "ItemCategory.Read";
    }

    public static class ItemComposition
    {
        public const string Read = "ItemComposition.Read";
    }

    public static class CompositionCategory
    {
        public const string Read = "CompositionCategory.Read";
    }

    public static class ProductionSchedule
    {
        public const string Read      = "ProductionSchedule.Read";
        public const string ChartRead = "ProductionChart.Read";
    }

    public static class ItemPricing
    {
        public const string Read = "ItemPricing.Read";
    }

    public static class MasterData
    {
        public const string Read = "MasterData.Read";
    }

    public static class Material
    {
        public const string Read = "Material.Read";
    }

    public static class Weather
    {
        public const string Read = "Weather.Read";
    }

    public static class Color
    {
        public const string Read = "Color.Read";
    }

    public static class Size
    {
        public const string Read = "Size.Read";
    }

    public static class Unit
    {
        public const string Read = "Unit.Read";
    }

    public static class Vehicle
    {
        public const string Read      = "Vehicle.Read";
        public const string ChartRead = "VehicleChart.Read";
    }

    public static class VehicleType
    {
        public const string Read = "VehicleType.Read";
    }

    public static class VehicleMaintenance
    {
        public const string Read = "VehicleMaintenance.Read";
    }

    public static class EquipmentCategory
    {
        public const string Read = "EquipmentCategory.Read";
    }

    public static class Equipment
    {
        public const string Read = "Equipment.Read";
    }

    public static class EquipmentMaintenance
    {
        public const string Read = "EquipmentMaintenance.Read";
    }

    public static class Warehouse
    {
        public const string Read = "Warehouse.Read";
    }

    public static class Inventory
    {
        public const string Read      = "Inventory.Read";
        public const string ChartRead = "InventoryChart.Read";
    }

    public static class WarehouseLocation
    {
        public const string Read = "WarehouseLocation.Read";
    }

    public static class InventoryTransaction
    {
        public const string Read = "InventoryTransaction.Read";
    }

    public static class InventoryTransactionType
    {
        public const string Read = "InventoryTransactionType.Read";
    }

    public static class MaterialIssue
    {
        public const string Read = "MaterialIssue.Read";
    }

    public static class InventoryStock
    {
        public const string Read = "InventoryStock.Read";
    }

    public static class InventoryReservation
    {
        public const string Read = "InventoryReservation.Read";
    }

    public static class StockTaking
    {
        public const string Read = "StockTaking.Read";
    }

    public static class ScaleRecord
    {
        public const string Read      = "ScaleRecord.Read";
        public const string ChartRead = "ScaleChart.Read";
    }

    public static class SetoffDocument
    {
        public const string Read      = "SetoffDocument.Read";
        public const string ChartRead = "FinancialChart.Read";
    }

    public static class PaymentMethod
    {
        public const string Read = "PaymentMethod.Read";
    }

    public static class Bank
    {
        public const string Read = "Bank.Read";
    }

    public static class BankStatement
    {
        public const string Read = "BankStatement.Read";
    }

    public static class Currency
    {
        public const string Read = "Currency.Read";
    }

    public static class Quotation
    {
        public const string Read = "Quotation.Read";
        public const string Approve = "Quotation.Approve";
    }

    public static class Sales
    {
        public const string Read      = "Sales.Read";
        public const string ChartRead = "SalesChart.Read";
    }

    public static class SalesOrder
    {
        public const string Read = "SalesOrder.Read";
        public const string Approve = "SalesOrder.Approve";
    }

    public static class SalesDelivery
    {
        public const string Read = "SalesDelivery.Read";
        public const string Approve = "SalesDelivery.Approve";
    }

    public static class SalesReturn
    {
        public const string Read = "SalesReturn.Read";
        public const string Approve = "SalesReturn.Approve";
    }

    public static class SalesReturnReason
    {
        public const string Read = "SalesReturnReason.Read";
    }

    public static class SalesTarget
    {
        public const string Read = "SalesTarget.Read";
    }

    public static class PurchaseOrder
    {
        public const string Read      = "PurchaseOrder.Read";
        public const string Approve   = "PurchaseOrder.Approve";
        public const string ChartRead = "PurchaseChart.Read";
    }

    public static class PurchaseReceiving
    {
        public const string Read = "PurchaseReceiving.Read";
        public const string Approve = "PurchaseReceiving.Approve";
    }

    public static class PurchaseReturn
    {
        public const string Read = "PurchaseReturn.Read";
        public const string Approve = "PurchaseReturn.Approve";
    }

    public static class PurchaseReturnReason
    {
        public const string Read = "PurchaseReturnReason.Read";
    }

    public static class AccountItem
    {
        public const string Read = "AccountItem.Read";
        public const string SubAccountRead = "SubAccount.Read";
        public const string SubAccountBatchCreate = "SubAccount.BatchCreate";
    }

    public static class JournalEntry
    {
        public const string Read = "JournalEntry.Read";
    }

    public static class FiscalPeriod
    {
        public const string Read = "FiscalPeriod.Read";
    }

    public static class Accounting
    {
        /// <summary>關帳 / 重開期間</summary>
        public const string ClosePeriod    = "Accounting.ClosePeriod";
        /// <summary>建立 / 編輯期初餘額傳票</summary>
        public const string OpeningBalance = "Accounting.OpeningBalance";
        /// <summary>傳票過帳</summary>
        public const string PostEntry      = "Accounting.PostEntry";
        /// <summary>執行年底結帳</summary>
        public const string YearEndClosing = "Accounting.YearEndClosing";
    }

    public static class Payroll
    {
        public const string Read         = "Payroll.Read";
        public const string ReadAmount   = "Payroll.ReadAmount";
        public const string Calculate    = "Payroll.Calculate";
        public const string Confirm      = "Payroll.Confirm";
        public const string Close        = "Payroll.Close";
        public const string SalaryConfig = "Payroll.SalaryConfig";
        public const string RateTable    = "Payroll.RateTable";
        public const string Declaration  = "Payroll.Declaration";
        public const string Attendance   = "Payroll.Attendance";
        public const string Payslip      = "Payroll.Payslip";
        public const string SelfView     = "Payroll.SelfView";
        public const string ChartRead    = "PayrollChart.Read";
    }

    // ===== Seeder 使用 =====

    /// <summary>
    /// 回傳所有權限定義，供 PermissionSeeder 同步至資料庫
    /// </summary>
    public static IEnumerable<PermissionDefinition> GetAllPermissions() =>
    [
        // ===== 敏感權限：系統 =====
        new(System.Admin,           "系統管理",        PermissionLevel.Sensitive, "系統最高管理權限，擁有所有功能存取權限",  "Nav.SystemGroup"),
        new(EmployeeAccount.Read,   "編輯員工帳號密碼", PermissionLevel.Sensitive, "編輯員工系統帳號、密碼與角色設定權限",    "Nav.HumanResources"),
        new(Permission.Read,        "檢視權限",        PermissionLevel.Sensitive, "檢視系統功能權限設定",                   "Nav.SystemGroup"),
        new(Role.Read,              "檢視角色",        PermissionLevel.Sensitive, "檢視系統角色與權限群組設定",              "Nav.SystemGroup"),
        new(Company.Read,           "檢視公司",        PermissionLevel.Sensitive, "檢視公司基本資料與相關資訊",              "Nav.SystemGroup"),
        new(SystemControl.Read,     "檢視系統控制",    PermissionLevel.Sensitive, "檢視系統設定與控制功能",                  "Nav.SystemGroup"),

        // ===== 敏感權限：採購審核 =====
        new(PurchaseOrder.Approve,    "審核採購訂單",   PermissionLevel.Sensitive, "審核與核准採購訂單權限",   "Nav.PurchaseGroup"),
        new(PurchaseReceiving.Approve,"審核採購進貨單", PermissionLevel.Sensitive, "審核與核准採購進貨單",     "Nav.PurchaseGroup"),
        new(PurchaseReturn.Approve,   "審核採購退回單", PermissionLevel.Sensitive, "審核與核准採購退回單",     "Nav.PurchaseGroup"),

        // ===== 敏感權限：銷貨審核 =====
        new(Quotation.Approve,        "審核報價單",     PermissionLevel.Sensitive, "審核與核准報價單",         "Nav.SalesGroup"),
        new(SalesOrder.Approve,       "審核銷貨訂單",   PermissionLevel.Sensitive, "審核與核准銷貨訂單",       "Nav.SalesGroup"),
        new(SalesDelivery.Approve,    "審核銷貨出貨單", PermissionLevel.Sensitive, "審核與核准銷貨出貨單",     "Nav.SalesGroup"),
        new(SalesReturn.Approve,      "審核銷貨退回單", PermissionLevel.Sensitive, "審核與核准銷貨退回單",     "Nav.SalesGroup"),

        // ===== 敏感權限：文件 =====
        new(Document.Sensitive,     "瀏覽敏感文件",    PermissionLevel.Sensitive, "瀏覽與下載敏感存取層級的文件",   "Nav.DocumentGroup"),
        new(Document.Manage,        "管理文件",        PermissionLevel.Sensitive, "上傳、編輯、刪除文件及管理檔案分類", "Nav.DocumentGroup"),

        // ===== 敏感權限：會計 =====
        new(AccountItem.SubAccountBatchCreate, "批次補建子科目", PermissionLevel.Sensitive, "批次補建客戶、廠商、品項子科目（高影響操作）", "Nav.AccountingGroup"),

        // ===== 一般權限：系統 =====
        new(PaperSetting.Read,              "檢視紙張設定",     PermissionLevel.Normal, "檢視紙張設定基本資料與相關資訊",   "Nav.SystemGroup"),
        new(ReportPrintConfiguration.Read,  "檢視報表列印設定", PermissionLevel.Normal, "檢視報表列印設定基本資料與相關資訊", "Nav.SystemGroup"),
        new(SystemParameter.Read,           "檢視系統參數",     PermissionLevel.Normal, "檢視系統全域參數設定",             "Nav.SystemGroup"),
        new(ErrorLog.Read,                  "檢視錯誤記錄",     PermissionLevel.Normal, "檢視系統錯誤記錄與問題追蹤",       "Nav.SystemGroup"),

        // ===== 一般權限：人力 =====
        new(User.Read,              "檢視使用者",   PermissionLevel.Normal, "檢視系統使用者基本資料",       "Nav.HumanResources"),
        new(Employee.Read,          "檢視員工",     PermissionLevel.Normal, "檢視員工基本資料與組織架構",         "Nav.HumanResources"),
        new(Employee.ChartRead,     "檢視員工圖表", PermissionLevel.Normal, "檢視員工統計分析圖表（主管層級）",   "Nav.HumanResources"),
        new(Department.Read,        "檢視部門",     PermissionLevel.Normal, "檢視公司部門組織架構資料",           "Nav.HumanResources"),
        new(EmployeePosition.Read,  "檢視員工職位", PermissionLevel.Normal, "檢視員工職位與職級設定",             "Nav.HumanResources"),

        // ===== 一般權限：客戶 =====
        new(Customer.Read,          "檢視客戶",     PermissionLevel.Normal, "檢視客戶基本資料與相關資訊",   "Nav.CustomerGroup"),
        new(Customer.ChartRead,     "檢視客戶圖表", PermissionLevel.Normal, "檢視客戶統計分析圖表（主管層級）", "Nav.CustomerGroup"),
        new(CustomerComplaint.Read, "檢視客訴",     PermissionLevel.Normal, "檢視客戶投訴紀錄與處理狀態",   "Nav.CustomerGroup"),
        new(CustomerType.Read,      "檢視客戶類型", PermissionLevel.Normal, "檢視客戶分類與類型設定",       "Nav.CustomerGroup"),

        // ===== 一般權限：廠商 =====
        new(Supplier.Read,      "檢視供應商",   PermissionLevel.Normal, "檢視供應商基本資料與相關資訊", "Nav.SupplierGroup"),
        new(Supplier.ChartRead, "檢視廠商圖表", PermissionLevel.Normal, "檢視廠商統計分析圖表（主管層級）", "Nav.SupplierGroup"),

        // ===== 一般權限：品項 =====
        new(Item.Read,               "檢視品項",         PermissionLevel.Normal, "檢視品項基本資料與規格",         "Nav.ItemGroup"),
        new(Item.ChartRead,          "檢視品項圖表",     PermissionLevel.Normal, "檢視品項統計分析圖表（主管層級）", "Nav.ItemGroup"),
        new(ItemCategory.Read,       "檢視品項分類",     PermissionLevel.Normal, "檢視品項分類階層與設定",         "Nav.ItemGroup"),
        new(ItemComposition.Read,    "檢視品項合成",     PermissionLevel.Normal, "檢視品項合成（BOM）結構與明細", "Nav.ItemGroup"),
        new(CompositionCategory.Read,   "檢視物料清單類型", PermissionLevel.Normal, "檢視品項物料清單的類型分類",     "Nav.ItemGroup"),
        new(ProductionSchedule.Read,     "檢視生產排程",     PermissionLevel.Normal, "檢視生產排程的詳細資料",           "Nav.ItemGroup"),
        new(ProductionSchedule.ChartRead,"檢視生產圖表",     PermissionLevel.Normal, "檢視生產管理統計分析圖表（主管層級）", "Nav.ItemGroup"),
        new(ItemPricing.Read,        "檢視品項定價",     PermissionLevel.Normal, "檢視品項價格設定與價格表",       "Nav.ItemGroup"),
        new(MasterData.Read,            "檢視基礎資料",     PermissionLevel.Normal, "檢視系統基礎資料維護功能",       "Nav.ItemGroup"),
        new(Material.Read,              "檢視材質",         PermissionLevel.Normal, "檢視品項材質分類與屬性",         "Nav.ItemGroup"),
        new(Weather.Read,               "檢視天氣",         PermissionLevel.Normal, "檢視天氣相關基礎資料",           "Nav.ItemGroup"),
        new(Color.Read,                 "檢視顏色",         PermissionLevel.Normal, "檢視顏色分類與色彩設定",         "Nav.ItemGroup"),
        new(Size.Read,                  "檢視尺寸",         PermissionLevel.Normal, "檢視尺寸規格與大小設定",         "Nav.ItemGroup"),
        new(Unit.Read,                  "檢視單位",         PermissionLevel.Normal, "檢視度量衡單位與換算設定",       "Nav.ItemGroup"),

        // ===== 一般權限：車輛 =====
        new(Vehicle.Read,           "檢視車輛",     PermissionLevel.Normal, "檢視車輛基本資料與相關資訊",     "Nav.VehicleGroup"),
        new(VehicleType.Read,       "檢視車輛類型", PermissionLevel.Normal, "檢視車輛類型基本資料與相關資訊", "Nav.VehicleGroup"),
        new(VehicleMaintenance.Read,"檢視保養紀錄", PermissionLevel.Normal, "檢視車輛保養紀錄與維修歷史",         "Nav.VehicleGroup"),
        new(Vehicle.ChartRead,      "檢視車輛圖表", PermissionLevel.Normal, "檢視車輛統計分析圖表（主管層級）",   "Nav.VehicleGroup"),

        // ===== 一般權限：設備 =====
        new(EquipmentCategory.Read,    "檢視設備類別", PermissionLevel.Normal, "檢視設備類別基本資料",                 "Nav.EquipmentGroup"),
        new(Equipment.Read,            "檢視設備",     PermissionLevel.Normal, "檢視設備基本資料與保養狀態",           "Nav.EquipmentGroup"),
        new(EquipmentMaintenance.Read, "檢視保養維修", PermissionLevel.Normal, "檢視設備保養維修記錄",                 "Nav.EquipmentGroup"),

        // ===== 一般權限：倉庫 =====
        new(Warehouse.Read,             "檢視倉庫",         PermissionLevel.Normal, "檢視倉庫基本資料與儲位設定",     "Nav.InventoryGroup"),
        new(Inventory.Read,             "檢視庫存",         PermissionLevel.Normal, "檢視庫存數量與庫存狀況",         "Nav.InventoryGroup"),
        new(Inventory.ChartRead,        "檢視庫存圖表",     PermissionLevel.Normal, "檢視庫存統計分析圖表（主管層級）", "Nav.InventoryGroup"),
        new(WarehouseLocation.Read,     "檢視倉庫位置",     PermissionLevel.Normal, "檢視倉庫內部位置與儲位設定",     "Nav.InventoryGroup"),
        new(InventoryTransaction.Read,  "檢視庫存異動",     PermissionLevel.Normal, "檢視庫存進出異動記錄",           "Nav.InventoryGroup"),
        new(InventoryTransactionType.Read,"檢視庫存異動類型",PermissionLevel.Normal, "檢視庫存異動類型與分類設定",     "Nav.InventoryGroup"),
        new(MaterialIssue.Read,         "檢視領料單",       PermissionLevel.Normal, "檢視領料單基本資料與明細",       "Nav.InventoryGroup"),
        new(InventoryStock.Read,        "檢視庫存明細",     PermissionLevel.Normal, "檢視詳細庫存明細與批號資訊",     "Nav.InventoryGroup"),
        new(InventoryReservation.Read,  "檢視庫存預留",     PermissionLevel.Normal, "檢視庫存預留與保留狀況",         "Nav.InventoryGroup"),
        new(StockTaking.Read,           "檢視盤點",         PermissionLevel.Normal, "檢視庫存盤點作業與結果",         "Nav.InventoryGroup"),

        // ===== 一般權限：磅秤紀錄 =====
        new(ScaleRecord.Read,      "檢視磅秤紀錄", PermissionLevel.Normal, "檢視磅秤紀錄基本資料與相關資訊",       "Nav.WasteGroup"),
        new(ScaleRecord.ChartRead, "檢視磅秤圖表", PermissionLevel.Normal, "檢視磅秤統計分析圖表（主管層級）",     "Nav.WasteGroup"),

        // ===== 一般權限：財務 =====
        new(SetoffDocument.Read,      "檢視沖款單",   PermissionLevel.Normal, "檢視客戶應收帳款與交易紀錄",         "Nav.FinanceGroup"),
        new(SetoffDocument.ChartRead, "檢視財務圖表", PermissionLevel.Normal, "檢視財務統計分析圖表（主管層級）",   "Nav.FinanceGroup"),
        new(PaymentMethod.Read, "檢視付款方式", PermissionLevel.Normal, "檢視系統付款方式設定",           "Nav.FinanceGroup"),
        new(Bank.Read,          "檢視銀行",     PermissionLevel.Normal, "檢視銀行基本資料與相關資訊",     "Nav.FinanceGroup"),
        new(Currency.Read,      "檢視貨幣",     PermissionLevel.Normal, "檢視貨幣基本資料與匯率設定",     "Nav.FinanceGroup"),
        new(BankStatement.Read, "檢視銀行對帳", PermissionLevel.Normal, "檢視銀行對帳單與配對傳票分錄",   "Nav.AccountingGroup"),

        // ===== 一般權限：銷貨 =====
        new(Quotation.Read,         "檢視報價單",       PermissionLevel.Normal, "檢視報價單與客戶交易紀錄",       "Nav.SalesGroup"),
        new(Sales.Read,             "檢視銷售訂單",     PermissionLevel.Normal, "檢視銷售訂單與客戶交易紀錄",     "Nav.SalesGroup"),
        new(Sales.ChartRead,        "檢視銷貨圖表",     PermissionLevel.Normal, "檢視銷貨統計分析圖表（主管層級）", "Nav.SalesGroup"),
        new(SalesOrder.Read,        "檢視銷貨訂單",     PermissionLevel.Normal, "檢視銷貨訂單詳細資料",           "Nav.SalesGroup"),
        new(SalesDelivery.Read,     "檢視銷貨出貨",     PermissionLevel.Normal, "檢視銷貨出貨單與配送記錄",       "Nav.SalesGroup"),
        new(SalesReturn.Read,       "檢視銷貨退回",     PermissionLevel.Normal, "檢視銷貨退回與退貨處理",         "Nav.SalesGroup"),
        new(SalesReturnReason.Read, "檢視銷貨退回原因", PermissionLevel.Normal, "檢視銷貨退回原因設定與管理",     "Nav.SalesGroup"),
        new(SalesTarget.Read,       "檢視業績目標",     PermissionLevel.Normal, "檢視與設定業務員業績目標",         "Nav.SalesGroup"),

        // ===== 一般權限：採購 =====
        new(PurchaseOrder.Read,         "檢視採購訂單",     PermissionLevel.Normal, "檢視採購訂單與供應商交易",       "Nav.PurchaseGroup"),
        new(PurchaseOrder.ChartRead,    "檢視採購圖表",     PermissionLevel.Normal, "檢視採購統計分析圖表（主管層級）", "Nav.PurchaseGroup"),
        new(PurchaseReceiving.Read,     "檢視採購收貨",     PermissionLevel.Normal, "檢視採購收貨單與驗收記錄",       "Nav.PurchaseGroup"),
        new(PurchaseReturn.Read,        "檢視採購退回貨",   PermissionLevel.Normal, "檢視採購退貨與退回處理",         "Nav.PurchaseGroup"),
        new(PurchaseReturnReason.Read,  "檢視採購退回原因", PermissionLevel.Normal, "檢視採購退回原因設定與管理",     "Nav.PurchaseGroup"),

        // ===== 一般權限：會計 =====
        new(AccountItem.Read,                  "檢視會計科目",   PermissionLevel.Normal,    "檢視會計科目與相關設定",                       "Nav.AccountingGroup"),
        new(AccountItem.SubAccountRead,        "檢視子科目設定", PermissionLevel.Normal,    "檢視子科目設定（統制科目代碼、自動產生開關）", "Nav.AccountingGroup"),
        new(JournalEntry.Read,                 "檢視傳票",       PermissionLevel.Normal,    "檢視會計傳票與分錄明細",                       "Nav.AccountingGroup"),
        new(FiscalPeriod.Read,                 "檢視會計期間",   PermissionLevel.Normal,    "檢視與管理會計期間（關帳、鎖定）",              "Nav.AccountingGroup"),
        new(Accounting.PostEntry,              "傳票過帳",       PermissionLevel.Sensitive, "執行傳票過帳（Draft → Posted）",               "Nav.AccountingGroup"),
        new(Accounting.ClosePeriod,            "關帳／重開期間", PermissionLevel.Sensitive, "執行會計期間關帳、鎖定及重新開放作業",         "Nav.AccountingGroup"),
        new(Accounting.OpeningBalance,         "期初餘額設定",   PermissionLevel.Sensitive, "建立或編輯期初餘額傳票",                       "Nav.AccountingGroup"),
        new(Accounting.YearEndClosing,         "年底結帳",       PermissionLevel.Sensitive, "執行年底結帳、損益科目歸零並轉保留盈餘",       "Nav.AccountingGroup"),

        // ===== 一般權限：文件 =====
        new(Document.Read, "瀏覽文件", PermissionLevel.Normal, "瀏覽與下載一般存取層級的文件", "Nav.DocumentGroup"),

        // ===== 一般權限：薪資 =====
        new(Payroll.Read,         "檢視薪資作業",   PermissionLevel.Normal,    "進入薪資計算作業頁面",                      "Nav.PayrollGroup"),
        new(Payroll.ReadAmount,   "查看薪資金額",   PermissionLevel.Sensitive, "查看本薪、實發薪資等金額（無此權限顯示***）", "Nav.PayrollGroup"),
        new(Payroll.Calculate,    "執行薪資計算",   PermissionLevel.Sensitive, "執行薪資試算與批次計算",                    "Nav.PayrollGroup"),
        new(Payroll.Confirm,      "確認薪資單",     PermissionLevel.Sensitive, "確認個人薪資單（試算→確認）",               "Nav.PayrollGroup"),
        new(Payroll.Close,        "薪資週期關帳",   PermissionLevel.Sensitive, "執行薪資週期關帳（關帳後不可修改）",         "Nav.PayrollGroup"),
        new(Payroll.SalaryConfig, "維護員工薪資設定", PermissionLevel.Sensitive, "設定員工本薪、津貼、勞健保等薪資參數",     "Nav.PayrollGroup"),
        new(Payroll.RateTable,    "維護薪資費率表", PermissionLevel.Sensitive, "維護薪資項目、基本工資、分級表等費率資料",   "Nav.PayrollGroup"),
        new(Payroll.Attendance,   "維護出勤彙總",   PermissionLevel.Sensitive, "輸入及維護員工每月出勤天數與加班時數",        "Nav.PayrollGroup"),
        new(Payroll.Payslip,      "列印薪資單",     PermissionLevel.Normal,    "列印或下載員工薪資單 PDF",                  "Nav.PayrollGroup"),
        new(Payroll.SelfView,     "查看本人薪資",   PermissionLevel.Normal,    "員工自助查看本人歷史薪資單（Phase 5）",      "Nav.PayrollGroup"),
    ];
}
