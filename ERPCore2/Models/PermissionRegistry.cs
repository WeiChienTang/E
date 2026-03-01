using ERPCore2.Models.Enums;

namespace ERPCore2.Models;

/// <summary>
/// 權限定義資料傳輸物件，供 PermissionSeeder 使用
/// </summary>
public record PermissionDefinition(string Code, string Name, PermissionLevel Level, string Remarks);

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

    public static class PrinterConfiguration
    {
        public const string Read = "PrinterConfiguration.Read";
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
        public const string Read = "Employee.Read";
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
    }

    public static class CustomerType
    {
        public const string Read = "CustomerType.Read";
    }

    public static class Supplier
    {
        public const string Read = "Supplier.Read";
    }

    public static class Product
    {
        public const string Read = "Product.Read";
    }

    public static class ProductCategory
    {
        public const string Read = "ProductCategory.Read";
    }

    public static class ProductComposition
    {
        public const string Read = "ProductComposition.Read";
    }

    public static class CompositionCategory
    {
        public const string Read = "CompositionCategory.Read";
    }

    public static class ProductionSchedule
    {
        public const string Read = "ProductionSchedule.Read";
    }

    public static class ProductPricing
    {
        public const string Read = "ProductPricing.Read";
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
        public const string Read = "Vehicle.Read";
    }

    public static class VehicleType
    {
        public const string Read = "VehicleType.Read";
    }

    public static class VehicleMaintenance
    {
        public const string Read = "VehicleMaintenance.Read";
    }

    public static class Warehouse
    {
        public const string Read = "Warehouse.Read";
    }

    public static class Inventory
    {
        public const string Read = "Inventory.Read";
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

    public static class WasteRecord
    {
        public const string Read = "WasteRecord.Read";
    }

    public static class WasteType
    {
        public const string Read = "WasteType.Read";
    }

    public static class SetoffDocument
    {
        public const string Read = "SetoffDocument.Read";
    }

    public static class PaymentMethod
    {
        public const string Read = "PaymentMethod.Read";
    }

    public static class Bank
    {
        public const string Read = "Bank.Read";
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
        public const string Read = "Sales.Read";
    }

    public static class SalesOrder
    {
        public const string Read = "SalesOrder.Read";
    }

    public static class SalesDelivery
    {
        public const string Read = "SalesDelivery.Read";
    }

    public static class SalesReturn
    {
        public const string Read = "SalesReturn.Read";
    }

    public static class SalesReturnReason
    {
        public const string Read = "SalesReturnReason.Read";
    }

    public static class PurchaseOrder
    {
        public const string Read = "PurchaseOrder.Read";
        public const string Approve = "PurchaseOrder.Approve";
    }

    public static class PurchaseReceiving
    {
        public const string Read = "PurchaseReceiving.Read";
    }

    public static class PurchaseReturn
    {
        public const string Read = "PurchaseReturn.Read";
    }

    public static class PurchaseReturnReason
    {
        public const string Read = "PurchaseReturnReason.Read";
    }

    public static class AccountItem
    {
        public const string Read = "AccountItem.Read";
    }

    public static class JournalEntry
    {
        public const string Read = "JournalEntry.Read";
    }

    // ===== Seeder 使用 =====

    /// <summary>
    /// 回傳所有權限定義，供 PermissionSeeder 同步至資料庫
    /// </summary>
    public static IEnumerable<PermissionDefinition> GetAllPermissions() =>
    [
        // ===== 敏感權限 =====
        new(System.Admin,           "系統管理",             PermissionLevel.Sensitive, "系統最高管理權限，擁有所有功能存取權限"),
        new(EmployeeAccount.Read,   "編輯員工帳號密碼",      PermissionLevel.Sensitive, "編輯員工系統帳號、密碼與角色設定權限"),
        new(Permission.Read,        "檢視權限",             PermissionLevel.Sensitive, "檢視系統功能權限設定"),
        new(Role.Read,              "檢視角色",             PermissionLevel.Sensitive, "檢視系統角色與權限群組設定"),
        new(Company.Read,           "檢視公司",             PermissionLevel.Sensitive, "檢視公司基本資料與相關資訊"),
        new(SystemControl.Read,     "檢視系統控制",          PermissionLevel.Sensitive, "檢視系統設定與控制功能"),
        new(PurchaseOrder.Approve,  "審核採購訂單",          PermissionLevel.Sensitive, "審核與核准採購訂單權限"),
        new(Quotation.Approve,      "審核報價單",            PermissionLevel.Sensitive, "審核與核准報價單"),
        new(Document.Sensitive,     "瀏覽敏感文件",          PermissionLevel.Sensitive, "瀏覽與下載敏感存取層級的文件"),
        new(Document.Manage,        "管理文件",              PermissionLevel.Sensitive, "上傳、編輯、刪除文件及管理檔案分類"),

        // ===== 一般權限：系統 =====
        new(PaperSetting.Read,              "檢視紙張設定",     PermissionLevel.Normal, "檢視紙張設定基本資料與相關資訊"),
        new(PrinterConfiguration.Read,      "檢視印表機設定",   PermissionLevel.Normal, "檢視印表機設定基本資料與相關資訊"),
        new(ReportPrintConfiguration.Read,  "檢視報表列印設定", PermissionLevel.Normal, "檢視報表列印設定基本資料與相關資訊"),
        new(SystemParameter.Read,           "檢視系統參數",     PermissionLevel.Normal, "檢視系統全域參數設定"),
        new(ErrorLog.Read,                  "檢視錯誤記錄",     PermissionLevel.Normal, "檢視系統錯誤記錄與問題追蹤"),

        // ===== 一般權限：人力 =====
        new(User.Read,              "檢視使用者",   PermissionLevel.Normal, "檢視系統使用者基本資料"),
        new(Employee.Read,          "檢視員工",     PermissionLevel.Normal, "檢視員工基本資料與組織架構"),
        new(Department.Read,        "檢視部門",     PermissionLevel.Normal, "檢視公司部門組織架構資料"),
        new(EmployeePosition.Read,  "檢視員工職位", PermissionLevel.Normal, "檢視員工職位與職級設定"),

        // ===== 一般權限：客戶 =====
        new(Customer.Read,      "檢視客戶",     PermissionLevel.Normal, "檢視客戶基本資料與相關資訊"),
        new(CustomerType.Read,  "檢視客戶類型", PermissionLevel.Normal, "檢視客戶分類與類型設定"),

        // ===== 一般權限：廠商 =====
        new(Supplier.Read, "檢視供應商", PermissionLevel.Normal, "檢視供應商基本資料與相關資訊"),

        // ===== 一般權限：商品 =====
        new(Product.Read,               "檢視商品",         PermissionLevel.Normal, "檢視商品基本資料與規格"),
        new(ProductCategory.Read,       "檢視商品分類",     PermissionLevel.Normal, "檢視商品分類階層與設定"),
        new(ProductComposition.Read,    "檢視商品合成",     PermissionLevel.Normal, "檢視商品合成（BOM）結構與明細"),
        new(CompositionCategory.Read,   "檢視物料清單類型", PermissionLevel.Normal, "檢視商品物料清單的類型分類"),
        new(ProductionSchedule.Read,    "檢視生產排程",     PermissionLevel.Normal, "檢視生產排程的詳細資料"),
        new(ProductPricing.Read,        "檢視商品定價",     PermissionLevel.Normal, "檢視商品價格設定與價格表"),
        new(MasterData.Read,            "檢視基礎資料",     PermissionLevel.Normal, "檢視系統基礎資料維護功能"),
        new(Material.Read,              "檢視材質",         PermissionLevel.Normal, "檢視商品材質分類與屬性"),
        new(Weather.Read,               "檢視天氣",         PermissionLevel.Normal, "檢視天氣相關基礎資料"),
        new(Color.Read,                 "檢視顏色",         PermissionLevel.Normal, "檢視顏色分類與色彩設定"),
        new(Size.Read,                  "檢視尺寸",         PermissionLevel.Normal, "檢視尺寸規格與大小設定"),
        new(Unit.Read,                  "檢視單位",         PermissionLevel.Normal, "檢視度量衡單位與換算設定"),

        // ===== 一般權限：車輛 =====
        new(Vehicle.Read,           "檢視車輛",     PermissionLevel.Normal, "檢視車輛基本資料與相關資訊"),
        new(VehicleType.Read,       "檢視車輛類型", PermissionLevel.Normal, "檢視車輛類型基本資料與相關資訊"),
        new(VehicleMaintenance.Read,"檢視保養紀錄", PermissionLevel.Normal, "檢視車輛保養紀錄與維修歷史"),

        // ===== 一般權限：倉庫 =====
        new(Warehouse.Read,             "檢視倉庫",         PermissionLevel.Normal, "檢視倉庫基本資料與儲位設定"),
        new(Inventory.Read,             "檢視庫存",         PermissionLevel.Normal, "檢視庫存數量與庫存狀況"),
        new(WarehouseLocation.Read,     "檢視倉庫位置",     PermissionLevel.Normal, "檢視倉庫內部位置與儲位設定"),
        new(InventoryTransaction.Read,  "檢視庫存異動",     PermissionLevel.Normal, "檢視庫存進出異動記錄"),
        new(InventoryTransactionType.Read,"檢視庫存異動類型",PermissionLevel.Normal, "檢視庫存異動類型與分類設定"),
        new(MaterialIssue.Read,         "檢視領料單",       PermissionLevel.Normal, "檢視領料單基本資料與明細"),
        new(InventoryStock.Read,        "檢視庫存明細",     PermissionLevel.Normal, "檢視詳細庫存明細與批號資訊"),
        new(InventoryReservation.Read,  "檢視庫存預留",     PermissionLevel.Normal, "檢視庫存預留與保留狀況"),
        new(StockTaking.Read,           "檢視盤點",         PermissionLevel.Normal, "檢視庫存盤點作業與結果"),

        // ===== 一般權限：廢料 =====
        new(WasteRecord.Read,   "檢視廢料記錄", PermissionLevel.Normal, "檢視廢料記錄基本資料與相關資訊"),
        new(WasteType.Read,     "檢視廢料類型", PermissionLevel.Normal, "檢視廢料類型基本資料與相關資訊"),

        // ===== 一般權限：財務 =====
        new(SetoffDocument.Read,"檢視沖款單",   PermissionLevel.Normal, "檢視客戶應收帳款與交易紀錄"),
        new(PaymentMethod.Read, "檢視付款方式", PermissionLevel.Normal, "檢視系統付款方式設定"),
        new(Bank.Read,          "檢視銀行",     PermissionLevel.Normal, "檢視銀行基本資料與相關資訊"),
        new(Currency.Read,      "檢視貨幣",     PermissionLevel.Normal, "檢視貨幣基本資料與匯率設定"),

        // ===== 一般權限：銷貨 =====
        new(Quotation.Read,         "檢視報價單",       PermissionLevel.Normal, "檢視報價單與客戶交易紀錄"),
        new(Sales.Read,             "檢視銷售訂單",     PermissionLevel.Normal, "檢視銷售訂單與客戶交易紀錄"),
        new(SalesOrder.Read,        "檢視銷貨訂單",     PermissionLevel.Normal, "檢視銷貨訂單詳細資料"),
        new(SalesDelivery.Read,     "檢視銷貨出貨",     PermissionLevel.Normal, "檢視銷貨出貨單與配送記錄"),
        new(SalesReturn.Read,       "檢視銷貨退回",     PermissionLevel.Normal, "檢視銷貨退回與退貨處理"),
        new(SalesReturnReason.Read, "檢視銷貨退回原因", PermissionLevel.Normal, "檢視銷貨退回原因設定與管理"),

        // ===== 一般權限：採購 =====
        new(PurchaseOrder.Read,         "檢視採購訂單",     PermissionLevel.Normal, "檢視採購訂單與供應商交易"),
        new(PurchaseReceiving.Read,     "檢視採購收貨",     PermissionLevel.Normal, "檢視採購收貨單與驗收記錄"),
        new(PurchaseReturn.Read,        "檢視採購退回貨",   PermissionLevel.Normal, "檢視採購退貨與退回處理"),
        new(PurchaseReturnReason.Read,  "檢視採購退回原因", PermissionLevel.Normal, "檢視採購退回原因設定與管理"),

        // ===== 一般權限：會計 =====
        new(AccountItem.Read,   "檢視會計科目", PermissionLevel.Normal, "檢視會計科目與相關設定"),
        new(JournalEntry.Read,  "檢視傳票",     PermissionLevel.Normal, "檢視會計傳票與分錄明細"),

        // ===== 一般權限：文件 =====
        new(Document.Read, "瀏覽文件", PermissionLevel.Normal, "瀏覽與下載一般存取層級的文件"),
    ];
}
